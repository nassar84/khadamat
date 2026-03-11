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
    private string appName = "خدمات";

    [ObservableProperty]
    private string appLogo = "app_logo.png";

    [ObservableProperty]
    private string userTitle = "دخول";

    [ObservableProperty]
    private string userImage = "profile_icon.png";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    private bool isAuthenticated = false;

    public bool IsNotAuthenticated => !IsAuthenticated;

    public void SetAuthenticated(bool value)
    {
        IsAuthenticated = value;
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
        
        await Shell.Current.GoToAsync(route);
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

    public async Task LoadSettingsAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://10.0.2.2:5144";
            
            // Fetch settings from API
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AppSettingsDto>>($"{baseUrl.TrimEnd('/')}/api/v1/settings");
            
            if (response?.Success == true && response.Data != null)
            {
                AppName = response.Data.ApplicationName;
                
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
