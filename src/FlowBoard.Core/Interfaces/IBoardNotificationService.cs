namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Service for sending real-time notifications to board subscribers.
/// </summary>
public interface IBoardNotificationService
{
    /// <summary>
    /// Notify clients that a task was created.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskData">The created task data (TaskItemDto)</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyTaskCreatedAsync(int boardId, object taskData, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a task was updated.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskData">The updated task data (TaskItemDto)</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyTaskUpdatedAsync(int boardId, object taskData, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a task was moved.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskId">The moved task ID</param>
    /// <param name="fromColumnId">Source column ID</param>
    /// <param name="toColumnId">Target column ID</param>
    /// <param name="newPosition">New position in target column</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyTaskMovedAsync(int boardId, int taskId, int fromColumnId, int toColumnId, int newPosition, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a task was deleted.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskId">The deleted task ID</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyTaskDeletedAsync(int boardId, int taskId, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a comment was added to a task.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskId">The task ID the comment was added to</param>
    /// <param name="commentData">The comment data (CommentDto)</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyCommentAddedAsync(int boardId, int taskId, object commentData, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a comment was updated.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskId">The task ID the comment belongs to</param>
    /// <param name="commentData">The updated comment data (CommentDto)</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyCommentUpdatedAsync(int boardId, int taskId, object commentData, string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that a comment was deleted.
    /// </summary>
    /// <param name="boardId">The board ID to notify</param>
    /// <param name="taskId">The task ID the comment belonged to</param>
    /// <param name="commentId">The deleted comment ID</param>
    /// <param name="excludeConnectionId">Connection ID to exclude (sender)</param>
    Task NotifyCommentDeletedAsync(int boardId, int taskId, int commentId, string? excludeConnectionId = null);
}
