using FlowBoard.API.Hubs;
using FlowBoard.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard.API.Services;

/// <summary>
/// SignalR implementation of the user notification service.
/// Sends real-time notifications to specific users via their user groups.
/// </summary>
public class SignalRUserNotificationService : IUserNotificationService
{
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly ILogger<SignalRUserNotificationService> _logger;

    public SignalRUserNotificationService(
        IHubContext<BoardHub> hubContext,
        ILogger<SignalRUserNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    private static string GetUserGroupName(int userId) => $"user_{userId}";

    public async Task SendNotificationAsync(int userId, object notificationData)
    {
        var groupName = GetUserGroupName(userId);
        _logger.LogInformation(
            "Sending notification to user {UserId}",
            userId);

        try
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync("NotificationReceived", notificationData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
        }
    }

    public async Task SendNotificationToMultipleAsync(IEnumerable<int> userIds, object notificationData)
    {
        var tasks = userIds.Select(userId => SendNotificationAsync(userId, notificationData));
        await Task.WhenAll(tasks);
    }
}
