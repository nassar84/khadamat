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
[IntentFilter(new[] { "OPEN_APP" }, Categories = new[] { global::Android.Content.Intent.CategoryDefault })]
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
        var route = intent.GetStringExtra("action_route");
        var title = intent.GetStringExtra("notification_title") ?? "";
        var message = intent.GetStringExtra("notification_message") ?? "";
        var reminderId = intent.GetIntExtra("reminder_id", 0);

        if (fromReminder || fromPush || !string.IsNullOrEmpty(route) || !string.IsNullOrEmpty(title))
        {
            var tapData = new Services.NotificationTapData
            {
                Title = title,
                Message = message,
                Route = route ?? "",
                ReminderId = reminderId,
                FromReminder = fromReminder,
                FromPush = fromPush
            };

            Console.WriteLine($"[MainActivity] Notification tapped: '{title}' (Route: '{route}', FromReminder: {fromReminder}, FromPush: {fromPush})");
            Services.DeepLinkBridge.RaiseNotificationTapped(tapData);
        }
    }
}
