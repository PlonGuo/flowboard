using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.API.Hubs;

/// <summary>
/// SignalR hub for real-time canvas/whiteboard collaboration.
/// </summary>
[Authorize]
public class CanvasHub : Hub
{
    private readonly ILogger<CanvasHub> _logger;

    public CanvasHub(ILogger<CanvasHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a canvas group to receive real-time updates.
    /// </summary>
    public async Task JoinCanvas(int canvasId)
    {
        var groupName = GetCanvasGroupName(canvasId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined canvas {CanvasId}", Context.UserIdentifier, canvasId);
    }

    /// <summary>
    /// Leave a canvas group.
    /// </summary>
    public async Task LeaveCanvas(int canvasId)
    {
        var groupName = GetCanvasGroupName(canvasId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} left canvas {CanvasId}", Context.UserIdentifier, canvasId);
    }

    /// <summary>
    /// Broadcast drawing operation to other users.
    /// </summary>
    public async Task BroadcastOperation(int canvasId, object operation)
    {
        await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
            .SendAsync("OperationReceived", operation);
    }

    /// <summary>
    /// Broadcast cursor position for collaborative awareness.
    /// </summary>
    public async Task BroadcastCursor(int canvasId, object cursorData)
    {
        await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
            .SendAsync("CursorMoved", new
            {
                UserId = Context.UserIdentifier,
                Data = cursorData
            });
    }

    /// <summary>
    /// Broadcast element selection for collaborative awareness.
    /// </summary>
    public async Task BroadcastSelection(int canvasId, string[] elementIds)
    {
        await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
            .SendAsync("SelectionChanged", new
            {
                UserId = Context.UserIdentifier,
                ElementIds = elementIds
            });
    }

    /// <summary>
    /// Notify that canvas data has been synced/saved.
    /// </summary>
    public async Task NotifyCanvasSaved(int canvasId, int version)
    {
        await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
            .SendAsync("CanvasSaved", new { canvasId, version });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Canvas client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Canvas client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetCanvasGroupName(int canvasId) => $"canvas-{canvasId}";
}
