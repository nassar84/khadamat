using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace Khadamat.MobileApp;

[Register("com.khadamat.app.MainActivity")]
[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try 
        {
            base.OnCreate(savedInstanceState);
            Plugin.Fingerprint.CrossFingerprint.SetCurrentActivityResolver(() => this);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: [ERROR] MainActivity OnCreate: {ex.Message}");
        }
    }
}
