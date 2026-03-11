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
        MainWebView.WebMessageReceived += MainWebView_WebMessageReceived;
    }

    private void MainWebView_WebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
    {
        var message = e.GetWebMessageAsString();
        if (message == "auth_success")
        {
            if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
            {
                vm.IsAuthenticated = true;
            }
        }
        else if (message == "auth_logout")
        {
            if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
            {
                vm.IsAuthenticated = false;
            }
        }
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

    public WebContainerPage(string route) : this()
    {
        _route = route;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: WebContainerPage OnNavigatedTo started");
        base.OnNavigatedTo(args);

        // Optional: Resolve base url from preferences or configuration if needed
        string baseUrl = Microsoft.Maui.Storage.Preferences.Default.Get("WebAppBaseUrl", "http://10.0.2.2:5144");
        
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
        _currentUrl = url;
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

        // Update current URL for refresh functionality
        _currentUrl = e.Url;

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
    protected override bool OnBackButtonPressed()
    {
        if (MainWebView != null && MainWebView.CanGoBack)
        {
            MainWebView.GoBack();
            return true; // Handled, don't exit the app
        }
        return base.OnBackButtonPressed();
    }
}
