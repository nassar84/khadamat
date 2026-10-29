using System;
using Microsoft.Maui.ApplicationModel;

namespace Khadamat.MobileApp.Services;

public class NotificationTapData
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Route { get; set; } = "";
    public int ReminderId { get; set; }
    public bool FromReminder { get; set; }
    public bool FromPush { get; set; }
    public DateTime TappedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Static bridge between native Android deep link / notification handling and Blazor navigation.
/// </summary>
public static class DeepLinkBridge
{
    public static event Action<string>? OnDeepLinkReceived;
    public static event Action<NotificationTapData>? OnNotificationTapped;

    public static string? PendingRoute { get; set; }
    public static NotificationTapData? PendingNotification { get; set; }

    public static void Raise(string uri)
    {
        PendingRoute = uri;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnDeepLinkReceived?.Invoke(uri);
        });
    }

    public static void RaiseNotificationTapped(NotificationTapData data)
    {
        PendingNotification = data;
        string effectiveRoute = !string.IsNullOrWhiteSpace(data.Route) && data.Route != "/" && data.Route != "//"
            ? data.Route
            : "notifications";
        PendingRoute = effectiveRoute;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnNotificationTapped?.Invoke(data);
        });
    }

    /// <summary>
    /// Converts deep link URIs to Blazor route paths.
    /// khadamat://service/120       → /service/120
    /// khadamat://category/5        → /categories/5
    /// https://khadamat.com/profile → /profile
    /// </summary>
    public static string ConvertToBlazorPath(string uri)
    {
        uri = (uri ?? "").Trim();

        if (uri.StartsWith("khadamat://"))
        {
            var path = uri.Replace("khadamat://", "/");
            return path.StartsWith("//") ? path[1..] : path;
        }

        if (Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
        {
            return parsedUri.PathAndQuery;
        }

        return uri.StartsWith("/") ? uri : "/" + uri;
    }
}
