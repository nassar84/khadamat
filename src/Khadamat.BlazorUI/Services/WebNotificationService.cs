using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services;

public class WebNotificationService : INotificationService
{
    public Task ShowNotificationAsync(NotificationRequest notification)
    {
        // Simple Console or Browser Alert fallback
        Console.WriteLine($"[Web Notification] {notification.Title}: {notification.Message}");
        return Task.CompletedTask;
    }

    public Task ScheduleNotificationAsync(NotificationRequest notification) => Task.CompletedTask;

    public Task CancelNotificationAsync(int notificationId) => Task.CompletedTask;

    public Task CancelAllNotificationsAsync() => Task.CompletedTask;

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public Task<string?> GetDeviceTokenAsync() => Task.FromResult<string?>(null);

    public Task SubscribeToTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
    public Task UnsubscribeFromTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
}
