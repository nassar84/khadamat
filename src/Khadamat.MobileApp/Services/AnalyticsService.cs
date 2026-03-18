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
        _isInitialized = true;
    }

    public Task TrackPageViewAsync(string pageName, Dictionary<string, string>? parameters = null)
    {
        EnsureInitialized();
        Console.WriteLine($"[Analytics Disabled] Page view: {pageName}");
        return Task.CompletedTask;
    }

    public Task TrackEventAsync(string eventName, Dictionary<string, string>? parameters = null)
    {
        EnsureInitialized();
        Console.WriteLine($"[Analytics Disabled] Event: {eventName}");
        return Task.CompletedTask;
    }

    public Task SetUserIdAsync(string userId)
    {
        Console.WriteLine($"[Analytics Disabled] SetUserId: {userId}");
        return Task.CompletedTask;
    }

    public Task SetUserPropertyAsync(string name, string value)
    {
        Console.WriteLine($"[Analytics Disabled] SetUserProperty: {name}={value}");
        return Task.CompletedTask;
    }
}
