using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace Khadamat.MobileApp.Platforms.Android;

/// <summary>
/// Firebase Cloud Messaging Service for Android.
/// Handles incoming push notifications from FCM.
/// Register in AndroidManifest.xml under <service android:name=".KhadamatFirebaseMessagingService">
/// </summary>
[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class KhadamatFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);

        // Save token securely for later use (e.g., register with your API)
        Preferences.Default.Set("fcm_device_token", token);
        Console.WriteLine($"[FCM] New Token: {token}");

        // TODO: Send token to your API backend
        // await _apiClient.RegisterDeviceTokenAsync(token);
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var title = message.GetNotification()?.Title ?? message.Data.GetValueOrDefault("title", "خدمات");
        var body = message.GetNotification()?.Body ?? message.Data.GetValueOrDefault("body", "");
        var navigateTo = message.Data.GetValueOrDefault("navigate_to", "");

        Console.WriteLine($"[FCM] Message received: {title} — {body}");

        // Show a local notification
        ShowLocalNotification(title, body, navigateTo);

        // Broadcast to the Blazor app so pages can react
        PushNotificationBridge.Raise(title, body, navigateTo);
    }

    private void ShowLocalNotification(string title, string body, string? deepLink)
    {
        const string channelId = "khadamat_channel";
        const int notificationId = 1001;

        var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        if (notificationManager == null) return;

        // Create channel (required Android 8+)
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new NotificationChannel(channelId, "إشعارات خدمات", NotificationImportance.High)
            {
                Description = "إشعارات التطبيق الرئيسية"
            };
            channel.EnableLights(true);
            channel.EnableVibration(true);
            notificationManager.CreateNotificationChannel(channel);
        }

        // Build the notification
        var builder = new Notification.Builder(this, channelId)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(Resource.Drawable.abc_btn_radio_material)
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.High);

        // Deep link on tap
        if (!string.IsNullOrEmpty(deepLink))
        {
            var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse($"khadamat://{deepLink}"));
            var pendingIntent = PendingIntent.GetActivity(
                this, 0, intent,
                PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);
            builder.SetContentIntent(pendingIntent);
        }

        notificationManager.Notify(notificationId, builder.Build());
    }
}
