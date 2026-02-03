using FlowBoard.API.Hubs;
using FlowBoard.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.API.Services;

/// <summary>
/// SignalR implementation of canvas notification service.
/// </summary>
public class SignalRCanvasNotificationService : ICanvasNotificationService
{
    private readonly IHubContext<CanvasHub> _hubContext;
    private readonly ILogger<SignalRCanvasNotificationService> _logger;

    public SignalRCanvasNotificationService(
        IHubContext<CanvasHub> hubContext,
        ILogger<SignalRCanvasNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyCanvasUpdatedAsync(
        int canvasId,
        string elements,
        string? appState,
        int version,
        string? excludeConnectionId = null)
    {
        try
        {
            var groupName = $"canvas-{canvasId}";
            var data = new
            {
                CanvasId = canvasId,
                Elements = elements,
                AppState = appState,
                Version = version,
                Timestamp = DateTime.UtcNow
            };

            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("CanvasUpdated", data);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("CanvasUpdated", data);
            }

            _logger.LogDebug("Notified canvas {CanvasId} updated to version {Version}", canvasId, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify canvas {CanvasId} update", canvasId);
        }
    }

    public async Task NotifyCanvasDeletedAsync(int canvasId, string? excludeConnectionId = null)
    {
        try
        {
            var groupName = $"canvas-{canvasId}";
            var data = new { CanvasId = canvasId, Timestamp = DateTime.UtcNow };

            if (string.IsNullOrEmpty(excludeConnectionId))
            {
                await _hubContext.Clients.Group(groupName).SendAsync("CanvasDeleted", data);
            }
            else
            {
                await _hubContext.Clients.GroupExcept(groupName, excludeConnectionId).SendAsync("CanvasDeleted", data);
            }

            _logger.LogDebug("Notified canvas {CanvasId} deleted", canvasId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify canvas {CanvasId} deletion", canvasId);
        }
    }
}
