using Android.App;
using Android.Content;
using Android.OS;

namespace Khadamat.MobileApp.Platforms.Android;

/// <summary>
/// Handles deep links of the form:
///   khadamat://service/120
///   khadamat://category/5
///   https://khadamat.com/service/120
/// 
/// Registered in AndroidManifest.xml with intent-filters for scheme "khadamat" and host "khadamat.com".
/// Uses DeepLinkBridge to forward the URI into Blazor's NavigationManager.
/// </summary>
[Activity(NoHistory = true, LaunchMode = global::Android.Content.PM.LaunchMode.SingleTask, Exported = true)]
public class DeepLinkActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleIntent(Intent);
        Finish(); // Close this transparent proxy activity
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
        Finish();
    }

    private void HandleIntent(Intent? intent)
    {
        if (intent?.Data == null) return;

        var uri = intent.Data.ToString();
        Console.WriteLine($"[DeepLink] Received: {uri}");

        if (!string.IsNullOrEmpty(uri))
        {
            DeepLinkBridge.Raise(uri);
        }
    }
}
