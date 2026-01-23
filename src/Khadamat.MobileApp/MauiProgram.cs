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
        // TODO: Update this to your actual API URL
        var apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
            ? "http://10.0.2.2:5144" // Android emulator localhost
            : "http://localhost:5144"; // iOS simulator
        
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

        // Blazored LocalStorage
        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddAuthorizationCore();

        return builder.Build();
    }
}
