using FlowBoard.Application.Commands.Tasks;
using FlowBoard.Application.Common;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Tasks;

/// <summary>
/// Handler for deleting a task.
/// </summary>
public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardNotificationService _notificationService;

    public DeleteTaskHandler(
        IUnitOfWork unitOfWork,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdWithCommentsAsync(request.Id);
        if (task == null)
        {
            return Result.Failure("Task not found");
        }

        // Get the board via column
        var board = await _unitOfWork.Boards.GetByIdAsync(task.Column.BoardId);
        if (board == null)
        {
            return Result.Failure("Board not found");
        }

        // Check permissions (task creator, board creator, or team owner)
        if (!await CanUserDeleteTask(task, board, request.UserId))
        {
            return Result.Failure("You don't have permission to delete this task");
        }

        // Get tasks in the same column that need position updates
        var tasksInColumn = await _unitOfWork.Tasks.GetByColumnIdAsync(task.ColumnId);
        var tasksToUpdate = tasksInColumn
            .Where(t => t.Position > task.Position)
            .ToList();

        // Save task ID and board ID before deletion for notification
        var taskId = task.Id;
        var boardId = board.Id;

        // Delete the task
        _unitOfWork.Tasks.Remove(task);

        // Update positions of remaining tasks
        foreach (var t in tasksToUpdate)
        {
            t.Position--;
            _unitOfWork.Tasks.Update(t);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify other clients viewing this board
        await _notificationService.NotifyTaskDeletedAsync(
            boardId,
            taskId,
            request.ConnectionId);

        return Result.Success();
    }

    private async Task<bool> CanUserDeleteTask(TaskItem task, Board board, int userId)
    {
        // Task creator can delete
        if (task.CreatedById == userId)
        {
            return true;
        }

        // Board creator can delete
        if (board.CreatedById == userId)
        {
            return true;
        }

        // Team owner can delete
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        return false;
    }
}
