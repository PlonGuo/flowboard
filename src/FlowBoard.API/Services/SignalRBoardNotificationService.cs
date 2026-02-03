using FlowBoard.API.Hubs;
using FlowBoard.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.API.Services;

/// <summary>
/// SignalR implementation of the board notification service.
/// Sends real-time notifications to clients viewing the same board.
/// </summary>
public class SignalRBoardNotificationService : IBoardNotificationService
{
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ILogger<SignalRBoardNotificationService> _logger;

    public SignalRBoardNotificationService(
        IHubContext<BoardHub> hubContext,
        ILogger<SignalRBoardNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    private static string GetBoardGroupName(int boardId) => $"board-{boardId}";

    public async Task NotifyTaskCreatedAsync(int boardId, object taskData, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        _logger.LogInformation(
            "Notifying TaskCreated to group {GroupName}, excluding connection {ConnectionId}",
            groupName, excludeConnectionId ?? "none");

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("TaskCreated", taskData);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("TaskCreated", taskData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TaskCreated notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyTaskUpdatedAsync(int boardId, object taskData, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        _logger.LogInformation(
            "Notifying TaskUpdated to group {GroupName}, excluding connection {ConnectionId}",
            groupName, excludeConnectionId ?? "none");

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("TaskUpdated", taskData);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("TaskUpdated", taskData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TaskUpdated notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyTaskMovedAsync(
        int boardId,
        int taskId,
        int fromColumnId,
        int toColumnId,
        int newPosition,
        string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        var payload = new { taskId, fromColumnId, toColumnId, newPosition };

        _logger.LogInformation(
            "Notifying TaskMoved to group {GroupName}: Task {TaskId} from column {FromColumn} to column {ToColumn} at position {Position}",
            groupName, taskId, fromColumnId, toColumnId, newPosition);

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("TaskMoved", payload);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("TaskMoved", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TaskMoved notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyTaskDeletedAsync(int boardId, int taskId, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        _logger.LogInformation(
            "Notifying TaskDeleted to group {GroupName}: Task {TaskId}",
            groupName, taskId);

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("TaskDeleted", taskId);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("TaskDeleted", taskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TaskDeleted notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyCommentAddedAsync(int boardId, int taskId, object commentData, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        var payload = new { taskId, comment = commentData };

        _logger.LogInformation(
            "Notifying CommentAdded to group {GroupName}: Task {TaskId}",
            groupName, taskId);

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("CommentAdded", payload);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("CommentAdded", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send CommentAdded notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyCommentUpdatedAsync(int boardId, int taskId, object commentData, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        var payload = new { taskId, comment = commentData };

        _logger.LogInformation(
            "Notifying CommentUpdated to group {GroupName}: Task {TaskId}",
            groupName, taskId);

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("CommentUpdated", payload);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("CommentUpdated", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send CommentUpdated notification to group {GroupName}", groupName);
        }
    }

    public async Task NotifyCommentDeletedAsync(int boardId, int taskId, int commentId, string? excludeConnectionId = null)
    {
        var groupName = GetBoardGroupName(boardId);
        var payload = new { taskId, commentId };

        _logger.LogInformation(
            "Notifying CommentDeleted to group {GroupName}: Task {TaskId}, Comment {CommentId}",
            groupName, taskId, commentId);

        try
        {
            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("CommentDeleted", payload);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("CommentDeleted", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send CommentDeleted notification to group {GroupName}", groupName);
        }
    }
}
