using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Khadamat.Application.DTOs;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Khadamat.Application.Common.Models;

namespace Khadamat.MobileApp.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string appName = "خدماتو";

    [ObservableProperty]
    private string appNameAr = "خدماتو";

    [ObservableProperty]
    private string appNameEn = "Khadamato";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string currentTab = "home";

    // Sound filenames
    [ObservableProperty]
    private string? openAppSound;
    [ObservableProperty]
    private string? findServiceSound;
    [ObservableProperty]
    private string? openDetailsSound;
    [ObservableProperty]
    private string? messageReceivedSound;
    [ObservableProperty]
    private string? notificationReceivedSound;

    [ObservableProperty]
    private string appLogo = "app_logo.png";

    [ObservableProperty]
    private string userTitle = "دخول";

    [ObservableProperty]
    private string userName = "مستخدم";

    [ObservableProperty]
    private string userImage = "app_logo.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    private bool isAuthenticated = false;

    public bool IsNotAuthenticated => !IsAuthenticated;

    [ObservableProperty]
    private bool isAdmin = false;

    [ObservableProperty]
    private bool isProvider = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClientMode))]
    private bool isProviderMode = false;

    public bool IsClientMode => !IsProviderMode;

    [RelayCommand]
    private void ToggleMode()
    {
        if (!IsProvider) return;
        IsProviderMode = !IsProviderMode;
        
        // Navigate based on mode
        if (IsProviderMode)
            _ = Shell.Current.GoToAsync("//HomePage?route=provider/dashboard");
        else
            _ = Shell.Current.GoToAsync("//HomePage");
    }

    public static event EventHandler? AuthChanged;

    public void SetAuthenticated(bool value, string? name = null, string? image = null, bool admin = false, bool provider = false)
    {
        IsAuthenticated = value;
        IsAdmin = admin;
        IsProvider = provider;
        
        // Default to client mode on login unless they are only a provider? 
        // We'll keep current mode or reset to Client for safety.
        IsProviderMode = false; 
        
        if (value)
        {
            UserName = !string.IsNullOrEmpty(name) ? name : "مستخدم";
            
            // Check if image looks like a real path; if not, use fallback logo instead of a missing file name
            if (!string.IsNullOrEmpty(image) && (image.Contains("/") || image.Contains(".") || image.StartsWith("http")))
                UserImage = image;
            else
                UserImage = "app_logo.png";
                
            UserTitle = string.IsNullOrEmpty(name) ? name : name.Split(' ')[0];
        }
        else
        {
            UserName = "زائر";
            UserTitle = "دخول";
            UserImage = "app_logo.png";
            IsAdmin = false;
            IsProvider = false;
        }

        AuthChanged?.Invoke(this, EventArgs.Empty);

        // Persist auth state
        var prefs = Microsoft.Maui.Storage.Preferences.Default;
        prefs.Set("IsAuthenticated", value);
        prefs.Set("IsAdmin", admin);
        prefs.Set("IsProvider", provider);
        
        if (value)
        {
            prefs.Set("UserName", UserName);
            prefs.Set("UserImage", UserImage);
        }
        else
        {
            prefs.Remove("UserName");
            prefs.Remove("UserImage");
        }
    }

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ShellViewModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Apply saved theme at startup
        var prefs = Microsoft.Maui.Storage.Preferences.Default;
        string savedTheme = prefs.Get("AppTheme", "default");
        ApplyThemeResources(savedTheme);

        // Restore auth state from preferences
        isAuthenticated = prefs.Get("IsAuthenticated", false);
        isAdmin = prefs.Get("IsAdmin", false);
        isProvider = prefs.Get("IsProvider", false);
        
        if (isAuthenticated)
        {
            userName = prefs.Get("UserName", "مستخدم");
            userImage = prefs.Get("UserImage", "app_logo.png");
            userTitle = userName.Split(' ')[0];
        }

        // Restore Brand Colors from Cache
        string cachedPrimary = prefs.Get("BrandPrimary", "");
        string cachedSecondary = prefs.Get("BrandSecondary", "");
        if (!string.IsNullOrEmpty(cachedPrimary) && savedTheme == "default")
        {
            try { 
                var res = Microsoft.Maui.Controls.Application.Current.Resources;
                res["Primary"] = Color.FromArgb(cachedPrimary);
                if (!string.IsNullOrEmpty(cachedSecondary)) res["Secondary"] = Color.FromArgb(cachedSecondary);
            } catch { }
        }
    }

    [RelayCommand]
    private async Task Navigate(string route)
    {
        if (string.IsNullOrEmpty(route) || IsBusy) return;
        
        try
        {
            IsBusy = true;
            Console.WriteLine($"ANTIGRAVITY_LOG: Navigating to {route}");

            // Close flyout first
            Shell.Current.FlyoutIsPresented = false;
            
            if (route == "logout")
            {
                if (!IsAuthenticated) return;
                
                try
                {
                    var cPage = Shell.Current.CurrentPage;
                    if (cPage is NavigationPage nPage) cPage = nPage.CurrentPage;
                    
                    if (cPage is Views.WebContainerPage wPage)
                    {
                        await wPage.ForceLogoutInWebView();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ANTIGRAVITY_LOG: Error clearing web auth: {ex.Message}");
                }

                SetAuthenticated(false);
                await Shell.Current.GoToAsync("//HomePage");
                return;
            }
            
            // Map the route to the exact Blazor web route path
            string blazorRoute = route;
            
            if (route == "marketplace")
                blazorRoute = "marketplace";
            else if (route == "profile")
                blazorRoute = "profile";
            else if (route == "login")
                blazorRoute = "login";
            else if (route == "register")
                blazorRoute = "register";
            else if (route == "favorites")
                blazorRoute = IsClientMode ? "client/favorites" : "provider/dashboard";
            else if (route == "messages")
                blazorRoute = "messages";
            else if (route == "provider/dashboard" || route == "my-services")
                blazorRoute = IsProviderMode ? "provider/services" : "provider/dashboard";
            else if (route == "services")
                blazorRoute = "client/services";
            else if (route == "settings")
                blazorRoute = "settings";
            else if (route == "admin")
                blazorRoute = "admin";
            else if (route == "admin/ads")
                blazorRoute = "admin/ads";
            else if (route == "terms")
                blazorRoute = "terms";
            else if (route == "home" || route == "//HomePage")
                blazorRoute = "";
            else if (route == "provider/apply")
                blazorRoute = "provider/apply";
            else if (route == "explore" || route == "categories")
                blazorRoute = "explore";
            else if (route == "support")
                blazorRoute = "contact";
            else if (route == "notifications" || route == "search")
                blazorRoute = route;
            else if (!route.StartsWith("//"))
                blazorRoute = route;
            else if (route.StartsWith("//"))
                blazorRoute = ""; // Fallback

            // Update CurrentTab for UI Highlighting enthusiastically (it will eventually be corrected by JS listener if wrong)
            var lowerRoute = blazorRoute.ToLower();
            if (lowerRoute.Contains("marketplace")) CurrentTab = "marketplace";
            else if (lowerRoute.Contains("favorite") || lowerRoute.Contains("my-services") || lowerRoute.Contains("provider/services")) CurrentTab = "favorites";
            else if (lowerRoute.Contains("messages")) CurrentTab = "messages";
            else if (lowerRoute.Contains("profile") || lowerRoute.Contains("login") || lowerRoute.Contains("register")) CurrentTab = "profile";
            else if (lowerRoute == "" || lowerRoute.Contains("home")) CurrentTab = "home";

            // Identify if we can do an inject-based navigation instead of MAUI shell push
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;
            
            if (currentPage is Views.WebContainerPage webPage)
            {
                Console.WriteLine($"ANTIGRAVITY_LOG: Routing inside WebView to: /{blazorRoute}");
                await webPage.NavigateToInternalRoute(blazorRoute);
            }
            else
            {
                // Fallback to native Home page load if we aren't already in one
                string routeParams = string.IsNullOrEmpty(blazorRoute) ? "" : $"?route={Uri.EscapeDataString(blazorRoute)}";
                await Shell.Current.GoToAsync($"//HomePage{routeParams}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Navigation Error for {route}: {ex.Message}");
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowThemePicker()
    {
        // Require CommunityToolkit.Maui.Views
        var popup = new Views.Popups.ThemePickerPopup();
        
        // Show Popup and get result
        var result = await CommunityToolkit.Maui.Views.PopupExtensions.ShowPopupAsync(
            Shell.Current.CurrentPage, 
            popup);

        string? theme = result as string;

        if (theme != null)
        {
            Microsoft.Maui.Storage.Preferences.Default.Set("AppTheme", theme);
            ApplyThemeResources(theme);
            
            // If we are currently on a web container, tell it to update JS immediately
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;
            
            if (currentPage is Views.WebContainerPage webPage)
            {
                await webPage.ApplyThemeToWebView(theme);
            }
        }
    }

    private void ApplyThemeResources(string theme)
    {
        // Hex codes from khadamat.css
        var colors = theme.ToLower() switch
        {
            "sunset" => ("#ff4e50", "#f9d423"),
            "ocean" => ("#00c6ff", "#0072ff"),
            "forest" => ("#00f260", "#0575e6"),
            "lavender" => ("#bf5af2", "#5e5ce6"),
            "royal" => ("#f093fb", "#f5576c"),
            _ => ("#6366f1", "#f43f5e") // Aurora / Default
        };

        if (Microsoft.Maui.Controls.Application.Current != null)
        {
            var res = Microsoft.Maui.Controls.Application.Current.Resources;
            res["Primary"] = Color.FromArgb(colors.Item1);
            res["Secondary"] = Color.FromArgb(colors.Item2);
            
            // Update TabBar and Header specifically if needed (handled via DynamicResource in XAML)
            Console.WriteLine($"ANTIGRAVITY_LOG: Global Theme Applied: {theme} (Primary: {colors.Item1})");
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            // Close the flyout menu
            Shell.Current.FlyoutIsPresented = false;

            // Find the current page
            var currentPage = Shell.Current.CurrentPage;
            
            // If it's a NavigationPage, we need the current page inside it
            if (currentPage is NavigationPage navPage)
            {
                currentPage = navPage.CurrentPage;
            }

            if (currentPage is Views.WebContainerPage webPage)
            {
                Console.WriteLine("ANTIGRAVITY_LOG: Refreshing App WebView via Native Shell Command");
                webPage.RefreshWebView();
            }
            else
            {
                 Console.WriteLine($"ANTIGRAVITY_LOG: Current page is not WebContainerPage. It is {currentPage?.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error during Refresh command: {ex.Message}");
        }
        
        await Task.CompletedTask;
    }

    [ObservableProperty]
    private string appSlogan = "بوابتك لأفضل الخدمات المهنية";

    public async Task LoadSettingsAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://10.0.2.2:5144";
            
            // Fetch settings from API
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AppSettingsDto>>($"{baseUrl.TrimEnd('/')}/v1/settings");
            
            if (response?.Success == true && response.Data != null)
            {
                AppName = response.Data.ApplicationName;
                AppNameAr = response.Data.ApplicationNameAr;
                AppNameEn = response.Data.ApplicationNameEn;
                
                OpenAppSound = response.Data.OpenAppSound;
                FindServiceSound = response.Data.FindServiceSound;
                OpenDetailsSound = response.Data.OpenDetailsSound;
                MessageReceivedSound = response.Data.MessageReceivedSound;
                NotificationReceivedSound = response.Data.NotificationReceivedSound;

                if (!string.IsNullOrEmpty(response.Data.WelcomeMessage))
                {
                    AppSlogan = response.Data.WelcomeMessage;
                }

                if (!string.IsNullOrEmpty(response.Data.PrimaryColor) && Microsoft.Maui.Controls.Application.Current != null)
                {
                    try
                    {
                        var primaryColor = Color.FromArgb(response.Data.PrimaryColor);
                        Microsoft.Maui.Controls.Application.Current.Resources["Primary"] = primaryColor;
                        
                        if (!string.IsNullOrEmpty(response.Data.SecondaryColor))
                        {
                            var secondaryColor = Color.FromArgb(response.Data.SecondaryColor);
                            Microsoft.Maui.Controls.Application.Current.Resources["Secondary"] = secondaryColor;
                        }

                        // Save to cache for next startup
                        var p = Microsoft.Maui.Storage.Preferences.Default;
                        p.Set("BrandPrimary", response.Data.PrimaryColor);
                        p.Set("BrandSecondary", response.Data.SecondaryColor);
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(response.Data.LogoUrl))
                {
                    // Ensure full URL for the image
                    if (response.Data.LogoUrl.StartsWith("http"))
                    {
                        AppLogo = response.Data.LogoUrl;
                    }
                    else
                    {
                        AppLogo = $"{baseUrl.TrimEnd('/')}/{response.Data.LogoUrl.TrimStart('/')}";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Error loading settings in ShellViewModel: {ex.Message}");
        }
    }
}
