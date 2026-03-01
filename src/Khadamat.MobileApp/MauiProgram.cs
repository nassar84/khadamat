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
            apiBaseUrl = "http://10.0.2.2:5144/";
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            apiBaseUrl = "http://localhost:5144/";
        }
        else
        {
            apiBaseUrl = "http://localhost:5144/";
        }
        
        // Print it to help with debugging
        Console.WriteLine($"ANTIGRAVITY_LOG: Using API Base URL: {apiBaseUrl}");
        
        builder.Services.AddTransient<AuthenticationHandler>();
        
        builder.Services.AddHttpClient("KhadamatAPI", client => 
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<AuthenticationHandler>();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("KhadamatAPI"));

        // Register Blazor UI Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ApiClient>();
        builder.Services.AddScoped<Khadamat.BlazorUI.Services.Admin.IAdminService, Khadamat.BlazorUI.Services.Admin.AdminService>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddScoped<SignalRClientService>();

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
        builder.Services.AddScoped<IBiometricService, MauiBiometricService>();
        builder.Services.AddScoped<IMobileAuthService, MobileAuthService>();
        builder.Services.AddScoped<SyncService>();
        // Blazored LocalStorage
        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddAuthorizationCore();

        return builder.Build();
    }
}
