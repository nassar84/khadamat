using Microsoft.Maui.Networking;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Khadamat.MobileApp.Views;

[QueryProperty(nameof(DeepLinkRoute), "route")]
public partial class WebContainerPage : ContentPage
{
    private string _deepLinkRoute = "";

    public string DeepLinkRoute
    {
        get => _deepLinkRoute;
        set
        {
            if (_deepLinkRoute != value)
            {
                _deepLinkRoute = value;
                // If the parameter is changed while the page is already there, force a load
                if (MainWebView != null) LoadContent(true);
            }
        }
    }
    protected string _route = string.Empty;

    public WebContainerPage()
    {
        InitializeComponent();
    }

    private string? _currentUrl;

    public void RefreshWebView()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (MainWebView != null)
            {
                Console.WriteLine("ANTIGRAVITY_LOG: Performing WebView Reload");
                // Option 1: Native Reload
                MainWebView.Reload();

                // Option 2: Fallback if Reload fails (re-assign source)
                if (!string.IsNullOrEmpty(_currentUrl))
                {
                    MainWebView.Source = new UrlWebViewSource { Url = _currentUrl };
                }
            }
        });
    }

    public async Task ForceLogoutInWebView()
    {
        try
        {
            if (MainWebView != null)
            {
                Console.WriteLine("ANTIGRAVITY_LOG: Clearing Auth state in WebView via JS injection");
                // Clear tokens from localStorage
                await MainWebView.EvaluateJavaScriptAsync("(function() { localStorage.removeItem('authToken'); localStorage.removeItem('refreshToken'); localStorage.removeItem('is_authenticated'); sessionStorage.clear(); })();");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error in ForceLogoutInWebView JS: {ex.Message}");
        }
    }

    public async Task NavigateToInternalRoute(string route)
    {
        try
        {
            if (MainWebView != null)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Injecting JS to navigate to '{route}'");
                await MainWebView.EvaluateJavaScriptAsync($@"
                    (function() {{
                        const newUrl = '/{route.TrimStart('/')}';
                        if (window.Blazor) {{
                            // Let the Blazor router take over without reloading
                            history.pushState(null, '', newUrl);
                            window.dispatchEvent(new Event('popstate'));
                        }} else {{
                            window.location.href = newUrl;
                        }}
                    }})();
                ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error navigating internal route JS: {ex.Message}");
        }
    }

    public WebContainerPage(string route) : this()
    {
        _route = route;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: WebContainerPage OnNavigatedTo started");
        base.OnNavigatedTo(args);
        // Always sync BottomNav auth state when any page becomes visible.
        // This fixes the case where login happened on ProfileTab but HomePage's BottomNav wasn't notified.
        BottomNav.RefreshAuthState();
        LoadContent(true); // Force reload when navigating to a page to ensure latest auth state
    }

    public void ReturnToRoot()
    {
        // Reset route to its original base route and reload
        DeepLinkRoute = "";
        LoadContent(true);
    }

    private void LoadContent(bool force = false)
    {
        // 1. Resolve base url from preferences or configuration
        string baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("WebAppBaseUrl", "https://jobsek.eis-dev.com");
        
        // Sync with API settings if available in ShellViewModel
        if (Shell.Current?.BindingContext is ViewModels.ShellViewModel vm)
        {
             // If we have a local dev URL or explicit override, use it
        }

        string routePart = !string.IsNullOrEmpty(DeepLinkRoute) ? DeepLinkRoute : (_route ?? "").TrimStart('/');

        string finalUrl = baseUrl.TrimEnd('/') + "/";

        // Use redirect parameter to avoid 404s on standalone WASM hosts
        if (!string.IsNullOrEmpty(routePart))
        {
            finalUrl += "?redirect=" + Uri.EscapeDataString(routePart);
        }

        // Append nativeapp=1 once to inform the Blazor side to hide website bars
        finalUrl += finalUrl.Contains("?") ? "&nativeapp=1" : "?nativeapp=1";

        LoadUrl(finalUrl, force);
    }

    private void LoadUrl(string url, bool force = false)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ShowOfflineState(true);
            return;
        }

        ShowOfflineState(false);

        // Add theme parameter if exists in preferences
        string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
        if (!url.Contains("theme="))
        {
            url += url.Contains("?") ? $"&theme={savedTheme}" : $"?theme={savedTheme}";
        }

        // Prevent reload if already on the same URL (especially for Home root)
        if (!force && MainWebView.Source is UrlWebViewSource current && current.Url == url)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: WebView already at target URL: {url}. Skipping reload.");
            return;
        }

        _currentUrl = url;
        
        // Save the base part for image loading in other components
        try 
        {
            var uri = new Uri(url);
            var basePart = uri.GetLeftPart(UriPartial.Authority);
            Microsoft.Maui.Storage.Preferences.Default.Set("WebAppBaseUrl", basePart);
        }
        catch { }

        Console.WriteLine($"ANTIGRAVITY_LOG: WebView loading URL: {url}");
        LoadingOverlay.IsVisible = true;
        MainWebView.Source = new UrlWebViewSource { Url = url };
    }

    public async Task ApplyThemeToWebView(string theme)
    {
        try
        {
            if (MainWebView != null && !string.IsNullOrEmpty(theme))
            {
                // Inject into sessionStorage and call the body theme helper defined in index.html
                await MainWebView.EvaluateJavaScriptAsync($"(function() {{ sessionStorage.setItem('theme', '{theme}'); if (window.setBodyTheme) {{ window.setBodyTheme('{theme}'); }} else {{ document.body.setAttribute('data-theme', '{theme}'); }} }})();");
                Console.WriteLine($"ANTIGRAVITY_LOG: Theme '{theme}' injected into WebView");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error applying theme JS: {ex.Message}");
        }
    }

    private void ShowOfflineState(bool isOffline)
    {
        OfflineOverlay.IsVisible = isOffline;
        PullToRefresh.IsVisible = !isOffline;
    }

    private void RetryButton_Clicked(object sender, EventArgs e)
    {
        if (MainWebView.Source is UrlWebViewSource urlSource)
        {
            LoadUrl(urlSource.Url ?? string.Empty);
        }
        else
        {
            LoadUrl(""); // Re-trigger initial load logic
        }
    }

    private void PullToRefresh_Refreshing(object sender, EventArgs e)
    {
        MainWebView.Reload();
    }

    private async void MainWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        var url = e.Url;

        // Catch internal history routing sync
        if (url.StartsWith("khadamat://routechange"))
        {
            e.Cancel = true;
            try {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var path = (query["path"] ?? "").ToLower().Trim('/');
                
                if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        if (path.Contains("marketplace")) vm.CurrentTab = "marketplace";
                        else if (path.Contains("favorites") || path.Contains("my-services") || path.Contains("provider/services")) vm.CurrentTab = "favorites";
                        else if (path.Contains("messages")) vm.CurrentTab = "messages";
                        else if (path.Contains("profile") || path.Contains("login") || path.Contains("register")) vm.CurrentTab = "profile";
                        else if (path == "" || path.Contains("home")) vm.CurrentTab = "home";
                    });
                }
            } catch {}
            LoadingOverlay.IsVisible = false;
            return;
        }

        // (The show loading moved down to after the interception checks)

        if (url.StartsWith("khadamat://auth/"))
        {
            e.Cancel = true;
            bool isSuccess = url.Contains("auth_success");
            bool isSync = url.Contains("auth_sync");

            if (isSuccess || isSync)
            {
                if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
                {
                    string name = "مستخدم";
                    string image = "profile_icon.png";
                    bool isAdmin = false;
                    bool isProvider = false;

                    // Parse data if available in query params
                    try
                    {
                        var uri = new Uri(url.Replace("khadamat://auth/", "http://localhost/").Replace("auth_success", "auth").Replace("auth_sync", "auth"));
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                        string? data = query["data"];
                        if (!string.IsNullOrEmpty(data))
                        {
                            var parts = data.Split('&');
                            foreach (var part in parts)
                            {
                                var firstEqual = part.IndexOf('=');
                                if (firstEqual > 0)
                                {
                                    var key = part.Substring(0, firstEqual);
                                    var val = part.Substring(firstEqual + 1);

                                    if (key == "name") name = Uri.UnescapeDataString(val);
                                    else if (key == "image") image = Uri.UnescapeDataString(val);
                                    else if (key == "is_admin") isAdmin = val == "true";
                                    else if (key == "is_provider") isProvider = val == "true";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ANTIGRAVITY_LOG: Error parsing auth data: {ex.Message}");
                    }

                    vm.SetAuthenticated(true, name, image, isAdmin, isProvider);

                    // Sync native bottom nav auth state
                    MainThread.BeginInvokeOnMainThread(() => BottomNav.RefreshAuthState());

                    // ONLY navigate to Home if it's a real login (auth_success), NOT for daily sync (auth_sync)
                    if (isSuccess)
                    {
                        MainThread.BeginInvokeOnMainThread(async () => {
                            await Shell.Current.GoToAsync("//HomePage");
                        });
                    }
                }
            }
            else if (url.Contains("auth_logout"))
            {
                if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
                {
                    vm.SetAuthenticated(false);
                    MainThread.BeginInvokeOnMainThread(() => BottomNav.RefreshAuthState());
                }
            }
            return;
        }

        // Smart Navigation Interception
        if (url.StartsWith("tel:"))
        {
            e.Cancel = true;
            if (Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.Default.IsSupported)
                Microsoft.Maui.ApplicationModel.Communication.PhoneDialer.Default.Open(url.Replace("tel:", ""));
            return;
        }

        if (url.StartsWith("mailto:"))
        {
            e.Cancel = true;
            await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(new Uri(url));
            return;
        }

        if (url.StartsWith("maps:") || url.Contains("google.com/maps"))
        {
            e.Cancel = true;
            await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(new Uri(url));
            return;
        }

        // Add parameter mobileapp=1 automatically contextually if not present?
        // Navigation within the same domain
        LoadingOverlay.IsVisible = true;
    }

    private async void MainWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        LoadingOverlay.IsVisible = false;
        PullToRefresh.IsRefreshing = false;

        if (e.Result != WebNavigationResult.Success)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                ShowOfflineState(true);
            }
            return;
        }

        // Update current URL for refresh functionality
        _currentUrl = e.Url;

        // Inject JS to persist nativeapp=1 across session and sync routing
        try
        {
            await MainWebView.EvaluateJavaScriptAsync(@"
                (function() {
                    // Save nativeapp flag in sessionStorage so Blazor can read it
                    sessionStorage.setItem('nativeapp', '1');
                    
                    if (window.__khadamat_nav_sync) return;
                    window.__khadamat_nav_sync = true;

                    function notifyMaui(url) {
                        try {
                            const iframe = document.createElement('iframe');
                            iframe.style.display = 'none';
                            iframe.src = 'khadamat://routechange?path=' + encodeURIComponent(url);
                            document.body.appendChild(iframe);
                            setTimeout(() => iframe.remove(), 200);
                        } catch(e) {}
                    }

                    const originalPush = history.pushState;
                    history.pushState = function() {
                        originalPush.apply(this, arguments);
                        notifyMaui(arguments[2] || location.pathname);
                    };

                    const originalReplace = history.replaceState;
                    history.replaceState = function() {
                        originalReplace.apply(this, arguments);
                        notifyMaui(arguments[2] || location.pathname);
                    };

                    window.addEventListener('popstate', () => {
                        notifyMaui(location.pathname + location.search);
                    });

                    // Send initial route configuration
                    notifyMaui(location.pathname + location.search);
                })();
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: JS injection error: {ex.Message}");
        }
    }
    protected override bool OnBackButtonPressed()
    {
        // 1. Try to go back in the WebView (Blazor's internal history)
        if (MainWebView != null && MainWebView.CanGoBack)
        {
            Console.WriteLine("ANTIGRAVITY_LOG: WebView can go back, navigating back internally.");
            MainWebView.GoBack();
            return true; // Prevent app exit
        }

        // 2. Try to go back in the Shell navigation stack (if we pushed any pages)
        if (Shell.Current.Navigation.NavigationStack.Count > 1)
        {
            Console.WriteLine("ANTIGRAVITY_LOG: Shell stack has items, popping page.");
            Shell.Current.Navigation.PopAsync();
            return true;
        }

        // 3. Fallback: If we are not on the HomePage root, go there instead of exiting
        var currentState = Shell.Current.CurrentState;
        var currentRoute = currentState.Location.ToString();
        
        // If we are on Marketplace/Favorites/Profile, return to Home Tab
        if (!currentRoute.EndsWith("//HomePage") && !currentRoute.EndsWith("//"))
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Not on HomePage ({currentRoute}), returning to Home Tab.");
            
            // If we are in Marketplace root, but want to go back to Home:
            Shell.Current.GoToAsync("//HomePage");
            return true;
        }

        // 4. Default: Exit the app (we are at the home root)
        Console.WriteLine("ANTIGRAVITY_LOG: No back history, exiting application.");
        return false;
    }
}

