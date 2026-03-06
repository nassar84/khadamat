namespace Khadamat.MobileApp.Services;

/// <summary>
/// Firebase Analytics service for tracking user behaviour in Blazor pages.
/// 
/// Register as Singleton in MauiProgram.cs:
///   builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
///
/// Usage in Blazor:
///   @inject IAnalyticsService Analytics
///   await Analytics.TrackPageViewAsync("Home");
///   await Analytics.TrackEventAsync("service_viewed", new() { ["service_id"] = "120" });
/// </summary>
public interface IAnalyticsService
{
    Task TrackPageViewAsync(string pageName, Dictionary<string, string>? parameters = null);
    Task TrackEventAsync(string eventName, Dictionary<string, string>? parameters = null);
    Task SetUserIdAsync(string userId);
    Task SetUserPropertyAsync(string name, string value);
}

public class AnalyticsService : IAnalyticsService
{
    private bool _isInitialized = false;

    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        // Firebase Analytics initializes automatically via google-services.json on Android.
        // No manual init needed — just use the static Firebase.Analytics.FirebaseAnalytics APIs.
        _isInitialized = true;
    }

    public Task TrackPageViewAsync(string pageName, Dictionary<string, string>? parameters = null)
    {
        EnsureInitialized();

        try
        {
#if ANDROID
            var analytics = Firebase.Analytics.FirebaseAnalytics.GetInstance(
                Android.App.Application.Context);

            var bundle = new Android.OS.Bundle();
            bundle.PutString(Firebase.Analytics.FirebaseAnalytics.Param.ScreenName, pageName);
            bundle.PutString(Firebase.Analytics.FirebaseAnalytics.Param.ScreenClass, pageName);

            if (parameters != null)
                foreach (var kv in parameters)
                    bundle.PutString(kv.Key, kv.Value);

            analytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.Event.ScreenView, bundle);
            Console.WriteLine($"[Analytics] Page view: {pageName}");
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] TrackPageView error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, string>? parameters = null)
    {
        EnsureInitialized();

        try
        {
#if ANDROID
            var analytics = Firebase.Analytics.FirebaseAnalytics.GetInstance(
                Android.App.Application.Context);

            var bundle = new Android.OS.Bundle();
            if (parameters != null)
                foreach (var kv in parameters)
                    bundle.PutString(kv.Key, kv.Value);

            analytics.LogEvent(eventName, bundle);
            Console.WriteLine($"[Analytics] Event: {eventName}");
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] TrackEvent error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task SetUserIdAsync(string userId)
    {
        try
        {
#if ANDROID
            var analytics = Firebase.Analytics.FirebaseAnalytics.GetInstance(
                Android.App.Application.Context);
            analytics.SetUserId(userId);
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] SetUserId error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task SetUserPropertyAsync(string name, string value)
    {
        try
        {
#if ANDROID
            var analytics = Firebase.Analytics.FirebaseAnalytics.GetInstance(
                Android.App.Application.Context);
            analytics.SetUserProperty(name, value);
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] SetUserProperty error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
