#if ANDROID
using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
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

        var imageUrl = message.GetNotification()?.ImageUrl?.ToString()
                       ?? (message.Data.ContainsKey("image_url") ? message.Data["image_url"] 
                       : (message.Data.ContainsKey("image") ? message.Data["image"] : null));

        Console.WriteLine($"[FCM] Push received: '{title}' - '{body}' (Image: {imageUrl}, Route: {actionRoute})");

        // Display standard system notification in notification tray
        ShowLocalNotification(title, body, actionRoute, imageUrl);

        // Broadcast to Blazor UI components if app is currently visible
        PushNotificationBridge.Raise(title, body, actionRoute);
    }

    private void ShowLocalNotification(string title, string body, string? actionRoute, string? imageUrl = null)
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
                channel.LightColor = global::Android.Graphics.Color.ParseColor("#6366f1");
                channel.EnableVibration(true);
                channel.SetShowBadge(true);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        // Tap action -> launches MainActivity with action_route
        var mainIntent = PackageManager?.GetLaunchIntentForPackage(PackageName)
                         ?? new Intent(this, Java.Lang.Class.FromType(typeof(MainActivity)));
        mainIntent.SetPackage(PackageName);
        mainIntent.SetAction(Intent.ActionMain);
        mainIntent.AddCategory(Intent.CategoryLauncher);
        mainIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        if (!string.IsNullOrEmpty(actionRoute) && actionRoute != "/" && actionRoute != "//")
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

        // Small Icon (white silhouette for status bar)
        int smallIcon = Resources?.GetIdentifier("ic_notification", "drawable", PackageName) ?? 0;
        if (smallIcon == 0)
        {
            smallIcon = ApplicationInfo?.Icon != 0 
                ? ApplicationInfo!.Icon 
                : global::Android.Resource.Drawable.SymDefAppIcon;
        }

        // Large Icon (app logo decoded into full-color Bitmap)
        Bitmap? largeIcon = null;
        try
        {
            int appIconRes = Resources?.GetIdentifier("khadamat_icon", "mipmap", PackageName) ?? 0;
            if (appIconRes == 0)
            {
                appIconRes = ApplicationInfo?.Icon ?? 0;
            }
            if (appIconRes != 0 && Resources != null)
            {
                largeIcon = BitmapFactory.DecodeResource(Resources, appIconRes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FCM] Error decoding large icon: {ex.Message}");
        }

        // Download rich notification image if provided
        Bitmap? bigPictureBitmap = null;
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            try
            {
                using var url = new Java.Net.URL(imageUrl);
                using var connection = url.OpenConnection();
                connection.ConnectTimeout = 4000;
                connection.ReadTimeout = 4000;
                using var input = connection.InputStream;
                bigPictureBitmap = BitmapFactory.DecodeStream(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FCM] Error downloading notification image: {ex.Message}");
            }
        }

        var defaultSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        var builder = new NotificationCompat.Builder(this, AndroidReminderScheduler.ChannelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSubText("خدماوي")
            .SetSmallIcon(smallIcon)
            .SetColor(global::Android.Graphics.Color.ParseColor("#6366f1"))
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetCategory(NotificationCompat.CategoryMessage)
            .SetSound(defaultSound)
            .SetVibrate(new long[] { 0, 250, 150, 250 })
            .SetLights(global::Android.Graphics.Color.ParseColor("#6366f1"), 1000, 500)
            .SetContentIntent(pendingIntent);

        if (bigPictureBitmap != null)
        {
            builder.SetStyle(new NotificationCompat.BigPictureStyle()
                .BigPicture(bigPictureBitmap)
                .SetBigContentTitle(title)
                .SetSummaryText(body));
        }
        else
        {
            builder.SetStyle(new NotificationCompat.BigTextStyle()
                .SetBigContentTitle(title)
                .BigText(body)
                .SetSummaryText("إشعار جديد"));
        }

        if (largeIcon != null)
        {
            builder.SetLargeIcon(largeIcon);
        }

        notificationManager.Notify(notificationId, builder.Build());
    }
}
#endif
