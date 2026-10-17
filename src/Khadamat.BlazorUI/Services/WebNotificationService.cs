using Khadamat.Shared.Interfaces;
using Khadamat.BlazorUI.Models;

namespace Khadamat.BlazorUI.Services;

public class WebNotificationService : INotificationService
{
    private readonly IFeedbackService _feedbackService;

    public WebNotificationService(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    public Task ShowNotificationAsync(NotificationRequest notification)
    {
        _feedbackService.ShowToast(notification.Title, notification.Message, FeedbackType.Info);
        return Task.CompletedTask;
    }

    public Task ScheduleNotificationAsync(NotificationRequest notification) => Task.CompletedTask;

    public Task CancelNotificationAsync(int notificationId) => Task.CompletedTask;

    public Task CancelAllNotificationsAsync() => Task.CompletedTask;

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public Task<string?> GetDeviceTokenAsync() => Task.FromResult<string?>(null);

    public Task SubscribeToTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
    public Task UnsubscribeFromTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
}
