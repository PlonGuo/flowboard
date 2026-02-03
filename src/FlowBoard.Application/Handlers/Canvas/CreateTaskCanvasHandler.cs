using AutoMapper;
using FlowBoard.Application.Commands.Canvas;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Canvas;

/// <summary>
/// Handler for creating a canvas for a task.
/// </summary>
public class CreateTaskCanvasHandler : IRequestHandler<CreateTaskCanvasCommand, Result<CanvasDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCanvasHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CanvasDetailDto>> Handle(
        CreateTaskCanvasCommand request,
        CancellationToken cancellationToken)
    {
        // Get the task
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            return Result.Failure<CanvasDetailDto>("Task not found");
        }

        // Get the column to find the board
        var column = await _unitOfWork.Columns.GetByIdAsync(task.ColumnId);
        if (column == null)
        {
            return Result.Failure<CanvasDetailDto>("Column not found");
        }

        // Get the board
        var board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
        if (board == null)
        {
            return Result.Failure<CanvasDetailDto>("Board not found");
        }

        // Verify user has access to the board's team
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<CanvasDetailDto>("You don't have permission to create a canvas for this task");
        }

        // Check if task already has a canvas
        var existingCanvas = await _unitOfWork.Canvases
            .FindAsync(c => c.TaskId == request.TaskId);

        if (existingCanvas.Any())
        {
            // Return existing canvas with data
            var existing = existingCanvas.First();
            var existingCreatedBy = await _unitOfWork.Users.GetByIdAsync(existing.CreatedById);
            var existingData = await _unitOfWork.CanvasData
                .FindAsync(cd => cd.CanvasId == existing.Id);

            var existingDto = new CanvasDetailDto
            {
                Id = existing.Id,
                Name = existing.Name,
                Description = existing.Description,
                BoardId = existing.BoardId,
                TaskId = existing.TaskId,
                TeamId = existing.TeamId,
                CreatedBy = existingCreatedBy != null ? _mapper.Map<UserSummaryDto>(existingCreatedBy) : null!,
                Data = existingData.Any() ? _mapper.Map<CanvasDataDto>(existingData.First()) : null,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
            return existingDto;
        }

        // Create the canvas
        var canvas = new Core.Entities.Canvas
        {
            Name = request.Name.Trim(),
            Description = $"Canvas for task: {task.Title}",
            TaskId = request.TaskId,
            TeamId = board.TeamId,
            CreatedById = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Canvases.AddAsync(canvas);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial canvas data
        var canvasData = new CanvasData
        {
            CanvasId = canvas.Id,
            Elements = "[]",
            AppState = null,
            Files = null,
            Version = 1,
            UpdatedById = request.UserId,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CanvasData.AddAsync(canvasData);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user for mapping
        var createdBy = await _unitOfWork.Users.GetByIdAsync(request.UserId);

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
            Data = _mapper.Map<CanvasDataDto>(canvasData),
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
