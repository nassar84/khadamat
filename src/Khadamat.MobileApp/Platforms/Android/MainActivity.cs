using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace Khadamat.MobileApp;

[Register("com.khadamat.app.MainActivity")]
[Activity(
    Name = "com.khadamat.app.MainActivity",
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    Exported = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
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
        }
        catch (Exception ex)
        {
            // Use native Android log in case Console.WriteLine is not attached
            Android.Util.Log.Error("MAUI_KHADAMAT", $"CRITICAL ERROR in OnCreate: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
