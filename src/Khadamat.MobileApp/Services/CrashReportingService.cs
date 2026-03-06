namespace Khadamat.MobileApp.Services;

/// <summary>
/// Crash reporting and error tracking service using Microsoft App Center.
///
/// Setup:
///   1. Add NuGet: Microsoft.AppCenter.Analytics + Microsoft.AppCenter.Crashes
///   2. In MauiProgram.cs call: AppCenter.Start("android=YOUR_KEY;", typeof(Analytics), typeof(Crashes));
///   3. Register this service as Singleton.
///
/// Usage in Blazor pages:
///   @inject ICrashReportingService CrashReporter
///   CrashReporter.TrackError(ex, "Service page failed to load");
/// </summary>
public interface ICrashReportingService
{
    void TrackError(Exception ex, string? context = null, Dictionary<string, string>? properties = null);
    void TrackEvent(string name, Dictionary<string, string>? properties = null);
    void SetUserId(string userId);
}

public class CrashReportingService : ICrashReportingService
{
    public void TrackError(Exception ex, string? context = null, Dictionary<string, string>? properties = null)
    {
        try
        {
            var props = properties ?? new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(context))
                props["context"] = context;

            props["timestamp"] = DateTime.UtcNow.ToString("o");
            props["platform"] = DeviceInfo.Platform.ToString();
            props["os_version"] = DeviceInfo.VersionString;
            props["app_version"] = AppInfo.VersionString;

            // Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, props);

            // Fallback: always log to console for debug
            Console.WriteLine($"[CrashReport] {context}: {ex.GetType().Name} — {ex.Message}");
            Console.WriteLine($"[CrashReport] Stack: {ex.StackTrace}");
        }
        catch (Exception loggingEx)
        {
            Console.WriteLine($"[CrashReport] Failed to report error: {loggingEx.Message}");
        }
    }

    public void TrackEvent(string name, Dictionary<string, string>? properties = null)
    {
        try
        {
            // Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, properties);
            Console.WriteLine($"[AppCenter] Event: {name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppCenter] TrackEvent error: {ex.Message}");
        }
    }

    public void SetUserId(string userId)
    {
        try
        {
            // AppCenter.SetUserId(userId);
            Console.WriteLine($"[AppCenter] UserId set: {userId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppCenter] SetUserId error: {ex.Message}");
        }
    }
}
