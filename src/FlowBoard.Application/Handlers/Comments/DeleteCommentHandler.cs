using FlowBoard.Application.Commands.Comments;
using FlowBoard.Application.Common;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Comments;

/// <summary>
/// Handler for deleting a comment.
/// </summary>
public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoardNotificationService _notificationService;

    public DeleteCommentHandler(
        IUnitOfWork unitOfWork,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<Unit>> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the comment
        var comments = await _unitOfWork.Comments.FindAsync(c => c.Id == request.CommentId);
        var comment = comments.FirstOrDefault();

        if (comment == null)
        {
            return Result.Failure<Unit>("Comment not found");
        }

        // Get the task, column, and board
        var task = await _unitOfWork.Tasks.GetByIdAsync(comment.TaskId);
        if (task == null)
        {
            return Result.Failure<Unit>("Task not found");
        }

        var column = await _unitOfWork.Columns.GetByIdAsync(task.ColumnId);
        if (column == null)
        {
            return Result.Failure<Unit>("Column not found");
        }

        var board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
        if (board == null)
        {
            return Result.Failure<Unit>("Board not found");
        }

        // Verify user is author or board owner/team owner
        var canDelete = await CanUserDeleteComment(comment, board, request.UserId);
        if (!canDelete)
        {
            return Result.Failure<Unit>("You don't have permission to delete this comment");
        }

        // Store task ID before deletion
        var taskId = comment.TaskId;

        // Delete the comment
        _unitOfWork.Comments.Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify other clients viewing this board
        await _notificationService.NotifyCommentDeletedAsync(
            board.Id,
            taskId,
            request.CommentId,
            request.ConnectionId);

        return Unit.Value;
    }

    private async Task<bool> CanUserDeleteComment(Comment comment, Board board, int userId)
    {
        // Author can delete their own comments
        if (comment.AuthorId == userId)
        {
            return true;
        }

        // Board creator can delete any comment
        if (board.CreatedById == userId)
        {
            return true;
        }

        // Team owner can delete any comment
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        return false;
    }
}
