using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// Notification data model
/// </summary>
public class NotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public int NotificationId { get; set; }
}

/// <summary>
/// Interface for push notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show local notification
    /// </summary>
    Task ShowNotificationAsync(NotificationRequest notification);

    /// <summary>
    /// Schedule notification for later
    /// </summary>
    Task ScheduleNotificationAsync(NotificationRequest notification);

    /// <summary>
    /// Cancel scheduled notification
    /// </summary>
    Task CancelNotificationAsync(int notificationId);

    /// <summary>
    /// Cancel all notifications
    /// </summary>
    Task CancelAllNotificationsAsync();

    /// <summary>
    /// Request notification permissions
    /// </summary>
    Task<bool> RequestPermissionAsync();

    /// <summary>
    /// Get device token for push notifications
    /// </summary>
    Task<string?> GetDeviceTokenAsync();
}
