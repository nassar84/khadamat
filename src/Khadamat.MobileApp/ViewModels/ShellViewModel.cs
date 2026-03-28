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
    private string userImage = "profile_icon.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    private bool isAuthenticated = false;

    public bool IsNotAuthenticated => !IsAuthenticated;

    [ObservableProperty]
    private bool isAdmin = false;

    [ObservableProperty]
    private bool isProvider = false;

    public void SetAuthenticated(bool value, string? name = null, string? image = null, bool admin = false, bool provider = false)
    {
        IsAuthenticated = value;
        IsAdmin = admin;
        IsProvider = provider;
        
        if (value)
        {
            UserName = !string.IsNullOrEmpty(name) ? name : "مستخدم";
            UserImage = !string.IsNullOrEmpty(image) ? image : "profile_icon.png";
            UserTitle = string.IsNullOrEmpty(name) ? "حسابي" : name.Split(' ')[0];
        }
        else
        {
            UserName = "زائر";
            UserTitle = "دخول";
            UserImage = "profile_icon.png";
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
        else if (route == "profile")
            route = "//ProfileTab";
        else if (route == "favorites")
            route = IsAuthenticated ? "favorites" : "//ProfileTab";
        else if (route == "messages")
            route = IsAuthenticated ? "messages" : "//ProfileTab";
        else if (route == "provider/dashboard")
            route = IsAuthenticated ? "provider/dashboard" : "//ProfileTab";
        else if (route == "my-services")
            route = IsAuthenticated ? "my-services" : "//ProfileTab";
        else if (route == "settings")
            route = IsAuthenticated ? "settings" : "//ProfileTab";
        else if (route == "explore")
            route = "//HomePage?route=explore";
        else if (route == "categories")
            route = "//HomePage?route=categories";
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
            
            // Set the native app theme color
            string primaryHex = theme switch
            {
                "sunset" => "#ea580c",
                "ocean" => "#0ea5e9",
                "forest" => "#10b981",
                "lavender" => "#8b5cf6",
                "royal" => "#eab308",
                _ => "#6366f1" // default
            };
            
            if (Microsoft.Maui.Controls.Application.Current != null)
            {
                Microsoft.Maui.Controls.Application.Current.Resources["Primary"] = Color.FromArgb(primaryHex);
            }
            
            // If we are currently on a web container, tell it to update JS immediately
            var currentPage = Shell.Current.CurrentPage;
            if (currentPage is NavigationPage navPage) currentPage = navPage.CurrentPage;
            
            if (currentPage is Views.WebContainerPage webPage)
            {
                await webPage.ApplyThemeToWebView(theme);
            }
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
