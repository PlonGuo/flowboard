namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Service for sending real-time canvas notifications via SignalR.
/// </summary>
public interface ICanvasNotificationService
{
    /// <summary>
    /// Notify clients that canvas data has been updated.
    /// </summary>
    Task NotifyCanvasUpdatedAsync(
        int canvasId,
        string elements,
        string? appState,
        int version,
        string? excludeConnectionId = null);

    /// <summary>
    /// Notify clients that canvas has been deleted.
    /// </summary>
    Task NotifyCanvasDeletedAsync(int canvasId, string? excludeConnectionId = null);
}
