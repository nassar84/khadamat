namespace Khadamat.MobileApp.Services;

/// <summary>
/// A static event bridge between native platform code (e.g., FCM) 
/// and Blazor components. Allows push notifications to trigger UI updates.
/// 
/// Usage in Blazor:
///     PushNotificationBridge.OnNotificationReceived += HandleNotification;
/// 
/// Raised by:
///     KhadamatFirebaseMessagingService.OnMessageReceived()
/// </summary>
public static class PushNotificationBridge
{
    public static event Action<PushNotificationPayload>? OnNotificationReceived;

    public static void Raise(string title, string body, string? navigateTo = null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnNotificationReceived?.Invoke(new PushNotificationPayload
            {
                Title = title,
                Body = body,
                NavigateTo = navigateTo
            });
        });
    }
}

public class PushNotificationPayload
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? NavigateTo { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.Now;
}
