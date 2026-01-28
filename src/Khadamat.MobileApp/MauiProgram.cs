using CommunityToolkit.Maui;
using Khadamat.BlazorUI.Services;
using Khadamat.BlazorUI.Services.Auth;
using Khadamat.BlazorUI.State;
using Khadamat.MobileApp.Services;
using Khadamat.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
// using Plugin.LocalNotification;
using Blazored.LocalStorage;

namespace Khadamat.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            //.UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Cairo-Regular.ttf", "CairoRegular");
                fonts.AddFont("Cairo-Bold.ttf", "CairoBold");
            });

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Configure HttpClient for API
        // Detect if running on emulator or physical device
        string apiBaseUrl;
        
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            // Check if running on emulator or physical device
            var isEmulator = DeviceInfo.DeviceType == DeviceType.Virtual;
            
            if (isEmulator)
            {
                // Android emulator - use special IP that maps to host machine's localhost
                apiBaseUrl = "http://10.0.2.2:5144";
            }
            else
            {
                // Physical Android device - use your PC's actual IP address
                // Make sure your phone and PC are on the same WiFi network
                apiBaseUrl = "http://10.102.2.2:5144";
            }
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // iOS simulator can use localhost
            apiBaseUrl = "http://localhost:5144";
        }
        else
        {
            // Fallback for other platforms
            apiBaseUrl = "http://localhost:5144";
        }
        
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        // Register Blazor UI Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ApiClient>();
        builder.Services.AddScoped<Khadamat.BlazorUI.Services.Admin.IAdminService, Khadamat.BlazorUI.Services.Admin.AdminService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddSingleton<AppState>();

        // Register Device Services
        builder.Services.AddSingleton<IDeviceCameraService, CameraService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<Khadamat.Shared.Interfaces.INotificationService, NotificationService>();
        builder.Services.AddSingleton<IPhoneService, PhoneService>();
        builder.Services.AddSingleton<IShareService, ShareService>();
        builder.Services.AddSingleton<IFilePickerService, FilePickerService>();
        builder.Services.AddSingleton<ISecureStorageService, MauiSecureStorageService>();
        builder.Services.AddSingleton<IExternalAuthService, MauiExternalAuthService>();
        builder.Services.AddSingleton<Khadamat.Application.Interfaces.IOfflineDataService, LocalDataService>();
        builder.Services.AddSingleton<IBiometricService, MauiBiometricService>();
        builder.Services.AddSingleton<IMobileAuthService, MobileAuthService>();
        builder.Services.AddSingleton<SyncService>();
        // Blazored LocalStorage
        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddAuthorizationCore();

        return builder.Build();
    }
}
