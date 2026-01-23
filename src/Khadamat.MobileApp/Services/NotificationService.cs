using Khadamat.Shared.Interfaces;
// using PluginLocalNotification = Plugin.LocalNotification;
// using PluginAndroid = Plugin.LocalNotification.AndroidOption;
// using PluginiOS = Plugin.LocalNotification.iOSOption;

namespace Khadamat.MobileApp.Services;

public class NotificationService : Khadamat.Shared.Interfaces.INotificationService
{
    public Task ShowNotificationAsync(Khadamat.Shared.Interfaces.NotificationRequest notification)
    {
        Console.WriteLine($"[Mock] ShowNotificationAsync: {notification.Title}");
        return Task.CompletedTask;
    }

    public Task ScheduleNotificationAsync(Khadamat.Shared.Interfaces.NotificationRequest notification)
    {
        Console.WriteLine($"[Mock] ScheduleNotificationAsync: {notification.Title}");
        return Task.CompletedTask;
    }

    public Task CancelNotificationAsync(int notificationId)
    {
        Console.WriteLine($"[Mock] CancelNotificationAsync: {notificationId}");
        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        Console.WriteLine("[Mock] CancelAllNotificationsAsync");
        return Task.CompletedTask;
    }

    public Task<bool> RequestPermissionAsync()
    {
        Console.WriteLine("[Mock] RequestPermissionAsync: Returning true");
        return Task.FromResult(true);
    }

    public Task<string?> GetDeviceTokenAsync()
    {
        return Task.FromResult(Preferences.Get("device_token", (string?)null));
    }
}
