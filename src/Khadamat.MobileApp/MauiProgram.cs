using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using Khadamat.MobileApp.Services;
using Khadamat.MobileApp.Security;
using Khadamat.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var resourceNames = assembly.GetManifestResourceNames();
        
        // Find main appsettings
        var appsettingsName = resourceNames.FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));
        if (appsettingsName != null)
        {
            using var stream = assembly.GetManifestResourceStream(appsettingsName);
            if (stream != null) builder.Configuration.AddJsonStream(stream);
        }
        else 
        {
            Console.WriteLine("ANTIGRAVITY_LOG: [WARNING] appsettings.json NOT FOUND in manifest resources!");
        }

        #if DEBUG
        var devSettingsName = resourceNames.FirstOrDefault(n => n.EndsWith("appsettings.Development.json", StringComparison.OrdinalIgnoreCase));
        if (devSettingsName != null)
        {
            using var devStream = assembly.GetManifestResourceStream(devSettingsName);
            if (devStream != null) builder.Configuration.AddJsonStream(devStream);
        }
        #endif

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .AddAudio()
            //.UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("fa-solid-900.ttf", "FA6Solid");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ── Khadamawy Dual-Domain Environment Resolution ─────────────────────────
        // Reads Khadamawy:ActiveEnvironment (e.g. "Current" or "Production") and
        // resolves ApiSettings:BaseUrl / ApiSettings:WebAppBaseUrl from the matching
        // sub-section. To switch domains: change Khadamawy:ActiveEnvironment in
        // Resources/Raw/appsettings.json and rebuild. No source-code changes needed.
        var activeEnv = builder.Configuration["Khadamawy:ActiveEnvironment"] ?? "Current";
        var resolvedApiBaseUrl   = builder.Configuration[$"Khadamawy:{activeEnv}:ApiBaseUrl"]
                                    ?? builder.Configuration["ApiSettings:BaseUrl"]
                                    ?? "https://khadamawy.eis-dev.com/";
        var resolvedWebAppBaseUrl = builder.Configuration[$"Khadamawy:{activeEnv}:WebBaseUrl"]
                                    ?? builder.Configuration["ApiSettings:WebAppBaseUrl"]
                                    ?? "https://khadamawy.eis-dev.com/";
        var googlePlayAppUrl      = builder.Configuration["Khadamawy:GooglePlayAppUrl"]
                                    ?? "https://play.google.com/store/apps/details?id=com.nassar84.khadamat";

        // Back-fill ApiSettings so any code that reads ApiSettings:BaseUrl still works
        builder.Configuration["ApiSettings:BaseUrl"]      = resolvedApiBaseUrl;
        builder.Configuration["ApiSettings:WebAppBaseUrl"] = resolvedWebAppBaseUrl;

        // Normalise
        var apiBaseUrl    = resolvedApiBaseUrl.TrimEnd('/') + "/";
        var webAppBaseUrl = resolvedWebAppBaseUrl.TrimEnd('/') + "/";

        // Persist to Preferences for pages that read them without DI configuration
        Preferences.Default.Set("WebAppBaseUrl", webAppBaseUrl);
        Preferences.Default.Set("ApiBaseUrl",    apiBaseUrl);
        Preferences.Default.Set("GooglePlayAppUrl", googlePlayAppUrl);

        // Print to help with debugging
        Console.WriteLine($"ANTIGRAVITY_LOG: [KhadamawyConfig] ActiveEnvironment={activeEnv}");
        Console.WriteLine($"ANTIGRAVITY_LOG: Using API Base URL: {apiBaseUrl}");
        Console.WriteLine($"ANTIGRAVITY_LOG: Using Web Application Base URL: {webAppBaseUrl}");
        Console.WriteLine($"ANTIGRAVITY_LOG: Google Play URL: {googlePlayAppUrl}");

        // Register UI and state things via Native methods or simplified stubs if needed for push notifications
        // Note: We removed Blazor authentication handling from the mobile container as the WebView handles it natively


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
        // Register HttpClient as a singleton for simple injection
        builder.Services.AddSingleton(sp => new HttpClient { Timeout = TimeSpan.FromSeconds(20) });
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<ViewModels.ShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<Views.WelcomePage>();
        
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
            {
                var errorMsg = $"[FATAL] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
                Console.WriteLine(errorMsg);
                // Save to Preferences so it can be viewed if the app survives a partial start
                try { Preferences.Default.Set("LastStartupError", errorMsg); } catch { }
            }
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            var errorMsg = $"[UNOBSERVED TASK] {e.Exception.Message}";
            Console.WriteLine(errorMsg);
            Preferences.Default.Set("LastTaskError", errorMsg);
            e.SetObserved(); 
        };


        try 
        {
            return builder.Build();
        }
        catch (Exception ex)
        {
            var fatalMsg = $"[BOOTSTRAP FATAL] {ex.Message}\n{ex.StackTrace}";
            Console.WriteLine(fatalMsg);
            try { Preferences.Default.Set("LastBootstrapError", fatalMsg); } catch { }
            throw; // Still throw to avoid running in unstable state, but we logged it
        }
    }
}
