#if ANDROID
using System;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using Khadamat.MobileApp.Services;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Platforms.Android;

/// <summary>
/// Firebase Cloud Messaging Service for Android.
/// Receives push notifications from FCM when app is in foreground or background.
/// </summary>
[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class KhadamatFirebaseMessagingService : FirebaseMessagingService
{
    public override async void OnNewToken(string token)
    {
        base.OnNewToken(token);

        try
        {
            await SecureStorage.Default.SetAsync("fcm_device_token", token);
            Preferences.Default.Set("fcm_device_token", token);
            Console.WriteLine($"[FCM] New token generated and stored: {token}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FCM] Error saving token: {ex.Message}");
        }
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var title = message.GetNotification()?.Title 
                    ?? (message.Data.ContainsKey("title") ? message.Data["title"] : "خدماوى");
        var body = message.GetNotification()?.Body 
                   ?? (message.Data.ContainsKey("body") ? message.Data["body"] : "");
        var actionRoute = message.Data.ContainsKey("action_route") ? message.Data["action_route"] 
                         : (message.Data.ContainsKey("navigate_to") ? message.Data["navigate_to"] : "/");

        Console.WriteLine($"[FCM] Push received: '{title}' - '{body}' (Route: {actionRoute})");

        // Display standard system notification in notification tray
        ShowLocalNotification(title, body, actionRoute);

        // Broadcast to Blazor UI components if app is currently visible
        PushNotificationBridge.Raise(title, body, actionRoute);
    }

    private void ShowLocalNotification(string title, string body, string? actionRoute)
    {
        var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        if (notificationManager == null) return;

        // Ensure channel exists
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var existingChannel = notificationManager.GetNotificationChannel(AndroidReminderScheduler.ChannelId);
            if (existingChannel == null)
            {
                var channel = new NotificationChannel(
                    AndroidReminderScheduler.ChannelId, 
                    AndroidReminderScheduler.ChannelName, 
                    NotificationImportance.High)
                {
                    Description = AndroidReminderScheduler.ChannelDescription
                };
                channel.EnableLights(true);
                channel.EnableVibration(true);
                channel.SetShowBadge(true);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        // Tap action -> launches MainActivity with action_route
        var mainIntent = new Intent(this, Java.Lang.Class.FromType(typeof(MainActivity)));
        mainIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        if (!string.IsNullOrEmpty(actionRoute))
        {
            mainIntent.PutExtra("action_route", actionRoute);
        }
        mainIntent.PutExtra("from_push", true);

        var notificationId = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);

        var pendingIntent = PendingIntent.GetActivity(
            this,
            notificationId,
            mainIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var icon = ApplicationInfo?.Icon != 0 
            ? ApplicationInfo!.Icon 
            : global::Android.Resource.Drawable.SymDefAppIcon;

        var builder = new NotificationCompat.Builder(this, AndroidReminderScheduler.ChannelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
            .SetSmallIcon(icon)
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetContentIntent(pendingIntent);

        notificationManager.Notify(notificationId, builder.Build());
    }
}
#endif
