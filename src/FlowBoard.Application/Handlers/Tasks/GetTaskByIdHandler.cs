using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Tasks;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Tasks;

/// <summary>
/// Handler for getting a task by ID.
/// </summary>
public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskItemDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTaskByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TaskItemDetailDto>> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = request.IncludeComments
            ? await _unitOfWork.Tasks.GetByIdWithCommentsAsync(request.Id)
            : await _unitOfWork.Tasks.GetByIdAsync(request.Id);

        if (task == null)
        {
            return Result.Failure<TaskItemDetailDto>("Task not found");
        }

        // Get the board via column to check access
        var board = await _unitOfWork.Boards.GetByIdAsync(task.Column?.BoardId ?? 0);
        if (board == null)
        {
            // Try loading the column if it wasn't loaded
            var column = await _unitOfWork.Columns.GetByIdAsync(task.ColumnId);
            if (column == null)
            {
                return Result.Failure<TaskItemDetailDto>("Column not found");
            }
            board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
            if (board == null)
            {
                return Result.Failure<TaskItemDetailDto>("Board not found");
            }
        }

        // Verify user has access
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<TaskItemDetailDto>("You don't have access to this task");
        }

        return _mapper.Map<TaskItemDetailDto>(task);
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
