using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;

namespace Khadamat.MobileApp;

// Required by MAUI WebAuthenticator to handle the khadamat://callback redirect
// Must match the callbackUrl used in WebAuthenticator.AuthenticateAsync()
[Register("com.nassar84.khadamat.WebAuthenticatorActivity")]
[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
// Handler for: khadamat://callback (host = "callback")
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "khadamat",
    DataHost = "callback")]
// Also handle the bare scheme: khadamat:// (no host)
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "khadamat")]
public class WebAuthenticatorActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
