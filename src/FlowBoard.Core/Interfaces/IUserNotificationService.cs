namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Service for sending real-time notifications to specific users.
/// </summary>
public interface IUserNotificationService
{
    /// <summary>
    /// Send a notification to a specific user.
    /// </summary>
    /// <param name="userId">The user ID to notify</param>
    /// <param name="notificationData">The notification data (NotificationDto)</param>
    Task SendNotificationAsync(int userId, object notificationData);

    /// <summary>
    /// Send notifications to multiple users.
    /// </summary>
    /// <param name="userIds">The user IDs to notify</param>
    /// <param name="notificationData">The notification data (NotificationDto)</param>
    Task SendNotificationToMultipleAsync(IEnumerable<int> userIds, object notificationData);
}
