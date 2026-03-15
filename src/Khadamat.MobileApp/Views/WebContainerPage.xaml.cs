using Microsoft.Maui.Networking;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Khadamat.MobileApp.Views;

[QueryProperty(nameof(DeepLinkRoute), "route")]
public partial class WebContainerPage : ContentPage
{
    private string _deepLinkRoute = "";
    private static bool _hasShownInitialLoad = false;
    private bool _isAnimating = false;
    private System.Threading.CancellationTokenSource? _animationCts;

    private readonly (string Image, string Title)[] _services = new[]
    {
        ("srv_electrician.png", "كهربائي محترف"),
        ("srv_teacher.png", "مدرس متميز"),
        ("srv_nurse.png", "ممرض مؤهل"),
        ("srv_driver.png", "سائق أمين")
    };
    public string DeepLinkRoute 
    { 
        get => _deepLinkRoute;
        set 
        {
            if (_deepLinkRoute != value)
            {
                _deepLinkRoute = value;
                // If the parameter is changed while the page is already there, force a load
                if (MainWebView != null) LoadContent();
            }
        }
    }
    protected string _route;

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

    public WebContainerPage(string route) : this()
    {
        _route = route;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: WebContainerPage OnNavigatedTo started");
        base.OnNavigatedTo(args);
        LoadContent();
    }

    public void ReturnToRoot()
    {
        // Reset route to its original base route and reload
        DeepLinkRoute = "";
        LoadContent();
    }

    private void LoadContent()
    {
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
        
        // Add theme parameter if exists in preferences
        string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
        if (!url.Contains("theme="))
        {
            url += url.Contains("?") ? $"&theme={savedTheme}" : $"?theme={savedTheme}";
        }

        _currentUrl = url;
        Console.WriteLine($"ANTIGRAVITY_LOG: WebView loading URL: {url}");
        StartLoadingAnimation();
        MainWebView.Source = new UrlWebViewSource { Url = url };
    }

    public async Task ApplyThemeToWebView(string theme)
    {
        try
        {
            if (MainWebView != null)
            {
                await MainWebView.EvaluateJavaScriptAsync($"(function() {{ sessionStorage.setItem('theme', '{theme}'); window.setBodyTheme('{theme}'); }})();");
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

        // Show loading only on the VERY FIRST navigation (app startup)
        if (!_hasShownInitialLoad && !url.Contains("#") && !LoadingOverlay.IsVisible)
        {
            LoadingOverlay.Opacity = 1;
            LoadingOverlay.IsVisible = true;
            StartLoadingAnimation();
        }

        if (url.StartsWith("khadamat://auth/"))
        {
            e.Cancel = true;
            if (url.Contains("auth_success"))
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
                        var uri = new Uri(url.Replace("khadamat://auth/", "http://localhost/"));
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        
                        string data = query["data"];
                        if (!string.IsNullOrEmpty(data))
                        {
                            var parts = data.Split('&');
                            foreach(var part in parts)
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
                }
            }
            else if (url.Contains("auth_logout"))
            {
                if (Shell.Current.BindingContext is ViewModels.ShellViewModel vm)
                    vm.SetAuthenticated(false);
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
    }

    private async void MainWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        PullToRefresh.IsRefreshing = false;
        
        // Hide loading overlay with animation for premium feel
        if (LoadingOverlay.IsVisible)
        {
            _hasShownInitialLoad = true; // Mark as shown so it doesn't appear on internal navigations
            StopLoadingAnimation();
            await Task.Delay(800); // Buffer to show the service images
            await LoadingOverlay.FadeTo(0, 400, Easing.CubicOut);
            LoadingOverlay.IsVisible = false;
            LoadingOverlay.Opacity = 1; // Reset for next time (if it were allowed, but flag prevents it)
        }
        
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

        // Inject JS to persist nativeapp=1 across session
        try
        {
            await MainWebView.EvaluateJavaScriptAsync(@"
                (function() {
                    // Save nativeapp flag in sessionStorage so Blazor can read it
                    sessionStorage.setItem('nativeapp', '1');
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

        // 3. Fallback: If we are not on the HomePage, go there instead of exiting
        var currentRoute = Shell.Current.CurrentState.Location.ToString();
        if (!currentRoute.Contains("HomePage") && !currentRoute.EndsWith("//"))
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Not on HomePage ({currentRoute}), returning to Home.");
            Shell.Current.GoToAsync("//HomePage");
            return true;
        }

        // 4. Default: Exit the app (we are at the home root)
        Console.WriteLine("ANTIGRAVITY_LOG: No back history, exiting application.");
        return false;
    }
    private void StartLoadingAnimation()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        _animationCts = new System.Threading.CancellationTokenSource();
        Task.Run(() => RotateServicesAsync(_animationCts.Token));
    }

    private void StopLoadingAnimation()
    {
        _isAnimating = false;
        _animationCts?.Cancel();
    }

    private async Task RotateServicesAsync(System.Threading.CancellationToken token)
    {
        int index = 0;
        while (!token.IsCancellationRequested)
        {
            var service = _services[index];
            
            await MainThread.InvokeOnMainThreadAsync(async () => {
                // Fade In + Scale Up
                ServiceAvatar.Source = service.Image;
                ServiceLabel.Text = service.Title;
                
                ServiceAvatar.Scale = 0.8;
                ServiceAvatar.Opacity = 0;
                ServiceLabel.Opacity = 0;
                ServiceLabel.TranslationY = 10;

                await Task.WhenAll(
                    ServiceAvatar.FadeTo(1, 600, Easing.CubicOut),
                    ServiceAvatar.ScaleTo(1.0, 600, Easing.CubicOut),
                    ServiceLabel.FadeTo(1, 600, Easing.CubicOut),
                    ServiceLabel.TranslateTo(0, 0, 600, Easing.CubicOut)
                );
            });

            // Wait while visible (unless cancelled)
            try { await Task.Delay(1500, token); } catch { break; }

            await MainThread.InvokeOnMainThreadAsync(async () => {
                // Fade Out + Scale Down
                await Task.WhenAll(
                    ServiceAvatar.FadeTo(0, 400, Easing.CubicIn),
                    ServiceAvatar.ScaleTo(0.9, 400, Easing.CubicIn),
                    ServiceLabel.FadeTo(0, 400, Easing.CubicIn)
                );
            });

            index = (index + 1) % _services.Length;
        }
    }
}
