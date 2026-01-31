using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.API.Hubs;

/// <summary>
/// SignalR hub for real-time board updates.
/// </summary>
[Authorize]
public class BoardHub : Hub
{
    private readonly ILogger<BoardHub> _logger;

    public BoardHub(ILogger<BoardHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a board group to receive real-time updates.
    /// </summary>
    public async Task JoinBoard(int boardId)
    {
        var groupName = GetBoardGroupName(boardId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined board {BoardId}", Context.UserIdentifier, boardId);
    }

    /// <summary>
    /// Leave a board group.
    /// </summary>
    public async Task LeaveBoard(int boardId)
    {
        var groupName = GetBoardGroupName(boardId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} left board {BoardId}", Context.UserIdentifier, boardId);
    }

    /// <summary>
    /// Notify clients that a task was created.
    /// </summary>
    public async Task NotifyTaskCreated(int boardId, object taskData)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("TaskCreated", taskData);
    }

    /// <summary>
    /// Notify clients that a task was updated.
    /// </summary>
    public async Task NotifyTaskUpdated(int boardId, object taskData)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("TaskUpdated", taskData);
    }

    /// <summary>
    /// Notify clients that a task was moved.
    /// </summary>
    public async Task NotifyTaskMoved(int boardId, int taskId, int fromColumnId, int toColumnId, int newPosition)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("TaskMoved", new { taskId, fromColumnId, toColumnId, newPosition });
    }

    /// <summary>
    /// Notify clients that a task was deleted.
    /// </summary>
    public async Task NotifyTaskDeleted(int boardId, int taskId)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("TaskDeleted", taskId);
    }

    /// <summary>
    /// Notify clients that a column was added.
    /// </summary>
    public async Task NotifyColumnCreated(int boardId, object columnData)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("ColumnCreated", columnData);
    }

    /// <summary>
    /// Notify clients that a column was updated.
    /// </summary>
    public async Task NotifyColumnUpdated(int boardId, object columnData)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("ColumnUpdated", columnData);
    }

    /// <summary>
    /// Notify clients that a column was deleted.
    /// </summary>
    public async Task NotifyColumnDeleted(int boardId, int columnId)
    {
        await Clients.OthersInGroup(GetBoardGroupName(boardId))
            .SendAsync("ColumnDeleted", columnId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetBoardGroupName(int boardId) => $"board-{boardId}";
}
