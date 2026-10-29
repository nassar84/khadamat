using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace Khadamat.MobileApp;

[Register("com.nassar84.khadamawy.MainActivity")]
[Activity(
    Name = "com.nassar84.khadamawy.MainActivity",
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { "OPEN_APP", "com.nassar84.khadamawy.NOTIFICATION_OPEN" }, Categories = new[] { global::Android.Content.Intent.CategoryDefault })]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try 
        {
            base.OnCreate(savedInstanceState);
            
            // Force RTL Layout at the Activity level to ensure the Toolbar and Flyout respect the Right-to-Left flow.
            if (Window != null && Window.DecorView != null)
            {
                Window.DecorView.LayoutDirection = Android.Views.LayoutDirection.Rtl;
            }

            // Plugin.Fingerprint setup
            Plugin.Fingerprint.CrossFingerprint.SetCurrentActivityResolver(() => this);

            // Handle incoming notification click intent
            HandleNotificationIntent(Intent);
        }
        catch (Exception ex)
        {
            // Use native Android log in case Console.WriteLine is not attached
            Android.Util.Log.Error("MAUI_KHADAMAT", $"CRITICAL ERROR in OnCreate: {ex.Message}\n{ex.StackTrace}");
        }
    }

    protected override void OnNewIntent(global::Android.Content.Intent? intent)
    {
        base.OnNewIntent(intent);
        Intent = intent;
        HandleNotificationIntent(intent);
    }

    private void HandleNotificationIntent(global::Android.Content.Intent? intent)
    {
        if (intent == null) return;

        var fromReminder = intent.GetBooleanExtra("from_reminder", false);
        var fromPush = intent.GetBooleanExtra("from_push", false);
        
        // Check for Google Play Services FCM background extras
        bool hasFcmMarker = intent.HasExtra("google.message_id") || 
                            intent.HasExtra("from") || 
                            intent.HasExtra("gcm.notification.title") ||
                            intent.HasExtra("google.c.a.e");
        
        if (hasFcmMarker)
        {
            fromPush = true;
        }

        // Comprehensive route resolution (from both foreground and background FCM payloads)
        var route = intent.GetStringExtra("action_route") 
                    ?? intent.GetStringExtra("navigate_to") 
                    ?? intent.GetStringExtra("route");

        // Comprehensive title resolution
        var title = intent.GetStringExtra("notification_title") 
                    ?? intent.GetStringExtra("title") 
                    ?? intent.GetStringExtra("gcm.notification.title") 
                    ?? "";

        // Comprehensive body/message resolution
        var message = intent.GetStringExtra("notification_message") 
                      ?? intent.GetStringExtra("body") 
                      ?? intent.GetStringExtra("gcm.notification.body") 
                      ?? intent.GetStringExtra("message") 
                      ?? "";

        var reminderId = intent.GetIntExtra("reminder_id", 0);

        // Normalize route: if it's empty, "/" or "//", point to the notifications inbox
        string effectiveRoute = !string.IsNullOrWhiteSpace(route) && route != "/" && route != "//"
            ? route
            : "notifications";

        if (fromReminder || fromPush || hasFcmMarker || !string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(message))
        {
            var tapData = new Services.NotificationTapData
            {
                Title = !string.IsNullOrWhiteSpace(title) ? title : "خدماوى",
                Message = message,
                Route = effectiveRoute,
                ReminderId = reminderId,
                FromReminder = fromReminder,
                FromPush = fromPush || hasFcmMarker
            };

            Console.WriteLine($"[MainActivity] Notification tapped: '{tapData.Title}' (Route: '{tapData.Route}', FromReminder: {fromReminder}, FromPush: {tapData.FromPush})");
            Services.DeepLinkBridge.RaiseNotificationTapped(tapData);
        }
    }
}
