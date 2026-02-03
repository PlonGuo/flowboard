using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Canvas;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Canvas;

/// <summary>
/// Handler for getting canvas by task ID.
/// </summary>
public class GetTaskCanvasHandler : IRequestHandler<GetTaskCanvasQuery, Result<CanvasDetailDto?>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTaskCanvasHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CanvasDetailDto?>> Handle(
        GetTaskCanvasQuery request,
        CancellationToken cancellationToken)
    {
        // Get the task
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            return Result.Failure<CanvasDetailDto?>("Task not found");
        }

        // Get the column to find the board
        var column = await _unitOfWork.Columns.GetByIdAsync(task.ColumnId);
        if (column == null)
        {
            return Result.Failure<CanvasDetailDto?>("Column not found");
        }

        // Get the board
        var board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
        if (board == null)
        {
            return Result.Failure<CanvasDetailDto?>("Board not found");
        }

        // Verify user has access
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<CanvasDetailDto?>("You don't have permission to view this task's canvas");
        }

        // Find canvas for this task
        var canvases = await _unitOfWork.Canvases
            .FindAsync(c => c.TaskId == request.TaskId);

        var canvas = canvases.FirstOrDefault();
        if (canvas == null)
        {
            // No canvas exists for this task - return null (not an error)
            return Result.Success<CanvasDetailDto?>(null);
        }

        // Load CreatedBy for mapping
        var createdBy = await _unitOfWork.Users.GetByIdAsync(canvas.CreatedById);

        // Get canvas data
        var canvasDataList = await _unitOfWork.CanvasData
            .FindAsync(cd => cd.CanvasId == canvas.Id);
        var canvasData = canvasDataList.FirstOrDefault();

        // Build DTO manually to avoid navigation property issues
        var dto = new CanvasDetailDto
        {
            Id = canvas.Id,
            Name = canvas.Name,
            Description = canvas.Description,
            BoardId = canvas.BoardId,
            TaskId = canvas.TaskId,
            TeamId = canvas.TeamId,
            CreatedBy = createdBy != null ? _mapper.Map<UserSummaryDto>(createdBy) : null!,
            Data = canvasData != null ? _mapper.Map<CanvasDataDto>(canvasData) : null,
            CreatedAt = canvas.CreatedAt,
            UpdatedAt = canvas.UpdatedAt
        };

        return dto;
    }

    private async Task<bool> UserHasAccessToBoard(Board board, int userId)
    {
        if (board.CreatedById == userId)
        {
            return true;
        }

        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId && tm.UserId == userId);

        return membership.Any();
    }
}
