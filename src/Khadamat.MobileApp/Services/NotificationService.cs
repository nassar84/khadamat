using Khadamat.Shared.Interfaces;
using PluginLocalNotification = Plugin.LocalNotification;
using PluginAndroid = Plugin.LocalNotification.AndroidOption;
using PluginiOS = Plugin.LocalNotification.iOSOption;

namespace Khadamat.MobileApp.Services;

public class NotificationService : Khadamat.Shared.Interfaces.INotificationService
{
    public async Task ShowNotificationAsync(Khadamat.Shared.Interfaces.NotificationRequest notification)
    {
        try
        {
            var hasPermission = await RequestPermissionAsync();
            if (!hasPermission) return;

            var request = new PluginLocalNotification.NotificationRequest
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Description = notification.Message,
                BadgeNumber = 1,
                Schedule = new PluginLocalNotification.NotificationRequestSchedule
                {
                    NotifyTime = notification.ScheduledTime ?? DateTime.Now.AddSeconds(1)
                },
                Android = new PluginAndroid.AndroidOptions
                {
                    ChannelId = "khadamat_channel",
                    Priority = PluginAndroid.AndroidPriority.High
                }
            };

            await PluginLocalNotification.LocalNotificationCenter.Current.Show(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notification error: {ex.Message}");
        }
    }

    public async Task ScheduleNotificationAsync(Khadamat.Shared.Interfaces.NotificationRequest notification)
    {
        if (notification.ScheduledTime == null)
        {
            notification.ScheduledTime = DateTime.Now.AddMinutes(1);
        }

        await ShowNotificationAsync(notification);
    }

    public async Task CancelNotificationAsync(int notificationId)
    {
        try
        {
            // Removed await in case it returns bool or void in newer versions
            PluginLocalNotification.LocalNotificationCenter.Current.Cancel(notificationId);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancel notification error: {ex.Message}");
        }
    }

    public async Task CancelAllNotificationsAsync()
    {
        try
        {
            // Removed await in case it returns bool or void
            PluginLocalNotification.LocalNotificationCenter.Current.ClearAll();
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancel all notifications error: {ex.Message}");
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            if (Microsoft.Maui.Devices.DeviceInfo.Platform == DevicePlatform.Android && Microsoft.Maui.Devices.DeviceInfo.Version.Major >= 13)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                }

                return status == PermissionStatus.Granted;
            }

            // iOS handles notification permissions automatically
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notification permission error: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> GetDeviceTokenAsync()
    {
        // This would be implemented with Firebase Cloud Messaging or similar
        // For now, return a placeholder
        await Task.CompletedTask;
        return Preferences.Get("device_token", null);
    }
}
