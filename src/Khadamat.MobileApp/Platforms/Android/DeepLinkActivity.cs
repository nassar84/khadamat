using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Khadamat.MobileApp.Services;

namespace Khadamat.MobileApp;

[Register("com.nassar84.khadamat.DeepLinkActivity")]
[Activity(NoHistory = true, LaunchMode = global::Android.Content.PM.LaunchMode.SingleTask, Exported = true)]
[IntentFilter(new[] { Intent.ActionView }, 
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, 
    DataScheme = "khadamat", 
    DataHost = "*", 
    AutoVerify = true)]
[IntentFilter(new[] { Intent.ActionView }, 
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, 
    DataScheme = "https", 
    DataHost = "jobsek.eis-dev.com", 
    AutoVerify = true)]
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
