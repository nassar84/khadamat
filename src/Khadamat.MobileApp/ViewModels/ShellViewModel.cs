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
    private string appName = "خدماوي";

    [ObservableProperty]
    private string appNameAr = "خدماوي";

    [ObservableProperty]
    private string appNameEn = "Khadamawi";

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
                
            UserTitle = string.IsNullOrEmpty(name) ? "حسابي" : name.Split(' ')[0];
        }
        else
        {
            UserName = "زائر";
            UserTitle = "دخول";
            UserImage = "app_logo.png";
            IsAdmin = false;
            IsProvider = false;
        }
    }

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ShellViewModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Apply saved theme at startup
        string savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "default");
        ApplyThemeResources(savedTheme);
    }

    [RelayCommand]
    private async Task Navigate(string route)
    {
        if (string.IsNullOrEmpty(route)) return;
        
        // Close flyout first
        Shell.Current.FlyoutIsPresented = false;
        
        if (route == "logout")
        {
            if (!IsAuthenticated) return;
            SetAuthenticated(false);
            // Inform Blazor part about logout via WebView if possible, or reload HomePage
            await Refresh();
            await Shell.Current.GoToAsync("//HomePage");
            return;
        }
        else if (route == "marketplace")
            route = IsClientMode ? "//MarketplacePage" : "//HomePage?route=marketplace";
        else if (route == "profile")
            route = "//ProfileTab";
        else if (route == "favorites")
            route = IsAuthenticated ? (IsClientMode ? "//FavoritesPage" : "//HomePage?route=client/favorites") : "//ProfileTab";
        else if (route == "messages")
            route = IsAuthenticated ? "//HomePage?route=messages" : "//ProfileTab";
        else if (route == "provider/dashboard" || route == "my-services")
            route = IsAuthenticated ? (IsProviderMode ? "//MyServicesPage" : "//HomePage?route=provider/dashboard") : "//ProfileTab";
        else if (route == "services")
            route = IsAuthenticated ? "//HomePage?route=services" : "//ProfileTab";
        else if (route == "settings")
            route = IsAuthenticated ? "//HomePage?route=settings" : "//ProfileTab";
        else if (route == "admin")
            route = "//HomePage?route=admin";
        else if (route == "admin/ads")
            route = "//HomePage?route=admin/ads";
        else if (route == "terms")
            route = "//HomePage?route=terms";
        else if (route == "home")
            route = "//HomePage";
        else if (route == "provider/apply")
            route = IsAuthenticated ? "//HomePage?route=provider/apply" : "//ProfileTab";
        else if (route == "explore" || route == "categories")
            route = IsClientMode ? "//CategoriesPage" : "//HomePage?route=explore";
        else if (route == "support")
        {
            await Shell.Current.GoToAsync($"//HomePage?route=contact"); 
            return;
        }
        else if (route == "notifications" || route == "search")
        {
            await Shell.Current.GoToAsync($"//HomePage?route={route}");
            return;
        }
        else if (route == "//HomePage")
            route = "//HomePage?route=";
        else if (!route.StartsWith("//"))
            route = "//HomePage?route=" + Uri.EscapeDataString(route);

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: Navigate Error for {route}: {ex.Message}");
            // Fallback to home
            await Shell.Current.GoToAsync("//HomePage");
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
