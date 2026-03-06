namespace Khadamat.MobileApp.Services;

/// <summary>
/// Static bridge between native Android deep link handling and Blazor navigation.
/// 
/// Flow:
///   Android URL → DeepLinkActivity → DeepLinkBridge.Raise() → Blazor NavigationManager
/// 
/// Usage in Routes.razor or App.razor:
///   protected override void OnInitialized()
///   {
///       DeepLinkBridge.OnDeepLinkReceived += HandleDeepLink;
///   }
///   
///   private void HandleDeepLink(string uri)
///   {
///       // "khadamat://service/120" → "/service/120"
///       var path = ParseDeepLinkPath(uri);
///       Navigation.NavigateTo(path);
///   }
/// </summary>
public static class DeepLinkBridge
{
    public static event Action<string>? OnDeepLinkReceived;

    public static void Raise(string uri)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnDeepLinkReceived?.Invoke(uri);
        });
    }

    /// <summary>
    /// Converts deep link URIs to Blazor route paths.
    /// khadamat://service/120       → /service/120
    /// khadamat://category/5        → /categories/5
    /// https://khadamat.com/profile → /profile
    /// </summary>
    public static string ConvertToBlazorPath(string uri)
    {
        uri = uri.Trim();

        if (uri.StartsWith("khadamat://"))
        {
            var path = uri.Replace("khadamat://", "/");
            return path.StartsWith("//") ? path[1..] : path;
        }

        if (uri.StartsWith("https://khadamat.com"))
        {
            return new Uri(uri).AbsolutePath;
        }

        return "/";
    }
}
