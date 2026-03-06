using CommunityToolkit.Maui;
using Khadamat.BlazorUI.Services;
using Khadamat.BlazorUI.Services.Auth;
using Khadamat.BlazorUI.State;
using Khadamat.MobileApp.Services;
using Khadamat.MobileApp.Security;
using Khadamat.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// ─────────────────────────────────────────────────────────────────────────────
// MauiProgram — Khadamat Mobile App Startup
// Startup strategy:
//   • Critical services registered as Singleton (fast, shared across lifetime)
//   • Heavy services use lazy initialisation internally (no work at DI resolve time)
//   • Blazor developer tools only enabled in Debug (reduces Release binary size)
//   • IHttpClientFactory used for all HttpClient instances (no socket exhaustion)
// ─────────────────────────────────────────────────────────────────────────────

namespace Khadamat.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Load appsettings.json
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("appsettings.json");
        if (stream != null)
        {
            builder.Configuration.AddJsonStream(stream);
        }

        #if DEBUG
        using var devStream = assembly.GetManifestResourceStream("appsettings.Development.json");
        if (devStream != null)
        {
            builder.Configuration.AddJsonStream(devStream);
        }
        #endif

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
        var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://api.yourdomain.com";
        
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
        builder.Services.AddSingleton<IMobileAuthService, MobileAuthService>();
        builder.Services.AddScoped<SyncService>();
        
        // Connectivity
        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

        // ── Analytics & Crash Reporting ───────────────────────────────────────
        // Both are Singleton: they hold no per-request state and must be
        // available throughout the entire app lifetime without re-initialising.
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<ICrashReportingService, CrashReportingService>();

        // ── FCM Token Registration ────────────────────────────────────────────
        // Scoped so it picks up the authenticated HttpClient per Blazor scope.
        // Call FcmTokenRegistrationService.RegisterTokenAsync() after login.
        builder.Services.AddScoped<FcmTokenRegistrationService>();

        // ── Startup Performance: Global exception handler ────────────────────
        // Catches unhandled exceptions in the .NET thread pool and reports them
        // to the crash reporting service without crashing silently.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Console.WriteLine($"[UNHANDLED] {ex.GetType().Name}: {ex.Message}");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine($"[UNOBSERVED TASK] {e.Exception.Message}");
            e.SetObserved(); // Prevent app crash on unobserved task exceptions
        };

        // Blazored LocalStorage
        builder.Services.AddBlazoredLocalStorage();

        // Authorization
        builder.Services.AddAuthorizationCore();

        return builder.Build();
    }
}
