using Microsoft.Maui.Networking;
using System;
using System.Threading.Tasks;

namespace Khadamat.MobileApp.Views;

[QueryProperty(nameof(DeepLinkRoute), "route")]
public partial class WebContainerPage : ContentPage
{
    public string DeepLinkRoute { get; set; } = "";
    protected string _route;

    public WebContainerPage()
    {
        InitializeComponent();
    }

    public WebContainerPage(string route) : this()
    {
        _route = route;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: WebContainerPage OnNavigatedTo started");
        base.OnNavigatedTo(args);

        // Optional: Resolve base url from preferences or configuration if needed
        // Assuming we set BaseUrl in secure storage or use a known constant for now
        // Let's use a standard domain based on the appsettings but for now hardcoded fallback route
        string baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("WebAppBaseUrl", "https://khadamat-app.vercel.app");
        
        string routePart = !string.IsNullOrEmpty(DeepLinkRoute) ? DeepLinkRoute : (_route ?? "").TrimStart('/');
        
        string finalUrl;
        if (string.IsNullOrEmpty(routePart))
            finalUrl = baseUrl.TrimEnd('/') + "/";
        else
            finalUrl = baseUrl.TrimEnd('/') + "/" + routePart;
        
        // Append nativeapp=1 once to inform the Blazor side to hide website bars
        finalUrl += finalUrl.Contains("?") ? "&nativeapp=1" : "?nativeapp=1";
        
        LoadUrl(finalUrl);
    }
    
    private void LoadUrl(string url)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            ShowOfflineState(true);
            return;
        }

        ShowOfflineState(false);
        MainWebView.Source = new UrlWebViewSource { Url = url };
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
            LoadUrl(urlSource.Url);
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
    }

    private async void MainWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        PullToRefresh.IsRefreshing = false;
        
        if (e.Result != WebNavigationResult.Success)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                ShowOfflineState(true);
            }
            return;
        }

        // Inject JS to persist nativeapp=1 across all internal navigation
        try
        {
            await MainWebView.EvaluateJavaScriptAsync(@"
                (function() {
                    // Save nativeapp flag in sessionStorage so Blazor can read it
                    sessionStorage.setItem('nativeapp', '1');

                    // Intercept all anchor clicks to inject nativeapp=1 into URL
                    if (!window._nativeAppInterceptorAttached) {
                        window._nativeAppInterceptorAttached = true;
                        document.addEventListener('click', function(e) {
                            var target = e.target;
                            while (target && target.tagName !== 'A') {
                                target = target.parentElement;
                            }
                            if (target && target.href) {
                                var url = new URL(target.href, window.location.href);
                                if (url.origin === window.location.origin) {
                                    if (!url.searchParams.has('nativeapp')) {
                                        url.searchParams.set('nativeapp', '1');
                                        target.href = url.toString();
                                    }
                                }
                            }
                        }, true);

                        // Also intercept Blazor's NavigationManager navigations
                        var _originalPushState = history.pushState;
                        history.pushState = function(state, title, url) {
                            if (url && typeof url === 'string') {
                                try {
                                    var u = new URL(url, window.location.href);
                                    if (!u.searchParams.has('nativeapp')) {
                                        u.searchParams.set('nativeapp', '1');
                                        url = u.pathname + u.search + u.hash;
                                    }
                                } catch(ex) {}
                            }
                            return _originalPushState.call(this, state, title, url);
                        };
                    }
                })();
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: JS injection error: {ex.Message}");
        }
    }
}
