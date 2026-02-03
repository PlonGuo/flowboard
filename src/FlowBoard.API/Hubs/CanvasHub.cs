using System.Collections.Concurrent;
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

    // Track users in each canvas with their assigned color
    private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, CanvasUserInfo>> _canvasUsers = new();

    // Color palette for user cursors
    private static readonly string[] CursorColors =
    [
        "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7",
        "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9"
    ];

    public CanvasHub(ILogger<CanvasHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a canvas group to receive real-time updates.
    /// </summary>
    public async Task JoinCanvas(int canvasId, string userName)
    {
        var groupName = GetCanvasGroupName(canvasId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        // Track user in canvas
        var canvasUsers = _canvasUsers.GetOrAdd(canvasId, _ => new ConcurrentDictionary<string, CanvasUserInfo>());
        var color = GetNextColor(canvasUsers.Count);
        var userInfo = new CanvasUserInfo
        {
            UserId = Context.UserIdentifier ?? "unknown",
            UserName = userName,
            Color = color,
            ConnectionId = Context.ConnectionId
        };
        canvasUsers.TryAdd(Context.ConnectionId, userInfo);

        // Notify others that user joined
        await Clients.OthersInGroup(groupName).SendAsync("UserJoined", userInfo);

        // Send current users list to the joining user
        await Clients.Caller.SendAsync("UsersInCanvas", canvasUsers.Values.ToList());

        _logger.LogInformation("User {UserId} ({UserName}) joined canvas {CanvasId}", Context.UserIdentifier, userName, canvasId);
    }

    /// <summary>
    /// Leave a canvas group.
    /// </summary>
    public async Task LeaveCanvas(int canvasId)
    {
        var groupName = GetCanvasGroupName(canvasId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        // Remove user from tracking
        if (_canvasUsers.TryGetValue(canvasId, out var canvasUsers))
        {
            if (canvasUsers.TryRemove(Context.ConnectionId, out var userInfo))
            {
                // Notify others that user left
                await Clients.OthersInGroup(groupName).SendAsync("UserLeft", userInfo.UserId);
            }
        }

        _logger.LogInformation("User {UserId} left canvas {CanvasId}", Context.UserIdentifier, canvasId);
    }

    /// <summary>
    /// Broadcast Excalidraw scene update to other users.
    /// </summary>
    public async Task BroadcastSceneUpdate(int canvasId, string elements, string? appState)
    {
        await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
            .SendAsync("SceneUpdated", new
            {
                UserId = Context.UserIdentifier,
                Elements = elements,
                AppState = appState,
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Broadcast pointer/cursor update for collaborative awareness.
    /// </summary>
    public async Task BroadcastPointerUpdate(int canvasId, double x, double y, string? selectedElementId)
    {
        if (_canvasUsers.TryGetValue(canvasId, out var canvasUsers) &&
            canvasUsers.TryGetValue(Context.ConnectionId, out var userInfo))
        {
            await Clients.OthersInGroup(GetCanvasGroupName(canvasId))
                .SendAsync("PointerUpdated", new
                {
                    userInfo.UserId,
                    userInfo.UserName,
                    userInfo.Color,
                    Pointer = new { X = x, Y = y, SelectedElementId = selectedElementId },
                    Timestamp = DateTime.UtcNow
                });
        }
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
    /// Broadcast cursor position for collaborative awareness (legacy).
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
        // Clean up user from all canvases they were in
        foreach (var kvp in _canvasUsers)
        {
            var canvasId = kvp.Key;
            var canvasUsers = kvp.Value;

            if (canvasUsers.TryRemove(Context.ConnectionId, out var userInfo))
            {
                await Clients.Group(GetCanvasGroupName(canvasId)).SendAsync("UserLeft", userInfo.UserId);
            }
        }

        _logger.LogInformation("Canvas client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private static string GetCanvasGroupName(int canvasId) => $"canvas-{canvasId}";

    private static string GetNextColor(int index) => CursorColors[index % CursorColors.Length];
}

/// <summary>
/// Info about a user in a canvas session.
/// </summary>
public class CanvasUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
}
