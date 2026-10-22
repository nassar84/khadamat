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
        var route = intent.GetStringExtra("action_route");
        if (!string.IsNullOrEmpty(route) && route != "/" && route != "//")
        {
            Console.WriteLine($"[MainActivity] Notification tapped. Target route: {route}");
            // Delay slightly to give the MAUI App and Shell time to mount (especially on cold boot)
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Services.DeepLinkBridge.Raise(route);
            });
        }
    }
}
