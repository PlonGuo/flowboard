using AutoMapper;
using FlowBoard.Application.Commands.Tasks;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Tasks;

/// <summary>
/// Handler for moving a task to a different column and/or position.
/// </summary>
public class MoveTaskHandler : IRequestHandler<MoveTaskCommand, Result<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBoardNotificationService _notificationService;

    public MoveTaskHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<TaskItemDto>> Handle(
        MoveTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(request.Id);
        if (task == null)
        {
            return Result.Failure<TaskItemDto>("Task not found");
        }

        // Check concurrency
        if (!task.RowVersion.SequenceEqual(request.RowVersion))
        {
            return Result.Failure<TaskItemDto>("The task has been modified by another user. Please refresh and try again.");
        }

        // Get the source board via current column
        var sourceBoard = await _unitOfWork.Boards.GetByIdAsync(task.Column.BoardId);
        if (sourceBoard == null)
        {
            return Result.Failure<TaskItemDto>("Board not found");
        }

        // Verify user has access
        if (!await UserHasAccessToBoard(sourceBoard, request.UserId))
        {
            return Result.Failure<TaskItemDto>("You don't have permission to move this task");
        }

        // Get the target column
        var targetColumn = await _unitOfWork.Columns.GetByIdAsync(request.ToColumnId);
        if (targetColumn == null)
        {
            return Result.Failure<TaskItemDto>("Target column not found");
        }

        // Verify target column is in the same board
        if (targetColumn.BoardId != sourceBoard.Id)
        {
            return Result.Failure<TaskItemDto>("Cannot move task to a different board");
        }

        var sourceColumnId = task.ColumnId;
        var sourcePosition = task.Position;
        var isSameColumn = sourceColumnId == request.ToColumnId;

        if (isSameColumn)
        {
            // Moving within the same column
            await ReorderWithinColumn(task, request.ToPosition, cancellationToken);
        }
        else
        {
            // Moving to a different column
            await MoveToDifferentColumn(task, sourceColumnId, sourcePosition, request.ToColumnId, request.ToPosition, cancellationToken);
        }

        // Update the task's position using entity method
        task.Move(request.ToColumnId, request.ToPosition);
        // Update RowVersion for concurrency control (PostgreSQL doesn't auto-generate)
        task.RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var movedTask = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(task.Id);

        // Notify other clients viewing this board
        await _notificationService.NotifyTaskMovedAsync(
            sourceBoard.Id,
            task.Id,
            sourceColumnId,
            request.ToColumnId,
            request.ToPosition,
            request.ConnectionId);

        return _mapper.Map<TaskItemDto>(movedTask);
    }

    private async Task ReorderWithinColumn(TaskItem task, int newPosition, CancellationToken cancellationToken)
    {
        var tasksInColumn = (await _unitOfWork.Tasks.GetByColumnIdAsync(task.ColumnId))
            .Where(t => t.Id != task.Id)
            .OrderBy(t => t.Position)
            .ToList();

        // Adjust positions of other tasks
        if (newPosition < task.Position)
        {
            // Moving up: shift tasks down
            foreach (var t in tasksInColumn.Where(t => t.Position >= newPosition && t.Position < task.Position))
            {
                t.Position++;
                _unitOfWork.Tasks.Update(t);
            }
        }
        else if (newPosition > task.Position)
        {
            // Moving down: shift tasks up
            foreach (var t in tasksInColumn.Where(t => t.Position > task.Position && t.Position <= newPosition))
            {
                t.Position--;
                _unitOfWork.Tasks.Update(t);
            }
        }
    }

    private async Task MoveToDifferentColumn(
        TaskItem task,
        int sourceColumnId,
        int sourcePosition,
        int targetColumnId,
        int targetPosition,
        CancellationToken cancellationToken)
    {
        // Shift tasks up in source column (close the gap)
        var sourceColumnTasks = await _unitOfWork.Tasks.GetByColumnIdAsync(sourceColumnId);
        foreach (var t in sourceColumnTasks.Where(t => t.Id != task.Id && t.Position > sourcePosition))
        {
            t.Position--;
            _unitOfWork.Tasks.Update(t);
        }

        // Shift tasks down in target column (make room)
        var targetColumnTasks = await _unitOfWork.Tasks.GetByColumnIdAsync(targetColumnId);
        foreach (var t in targetColumnTasks.Where(t => t.Position >= targetPosition))
        {
            t.Position++;
            _unitOfWork.Tasks.Update(t);
        }
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
