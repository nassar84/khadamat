using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services;

public class WebNotificationService : INotificationService
{
    public Task ShowNotificationAsync(string title, string message)
    {
        // On web we could use Browser Notifications API or a simple alert
        return Task.CompletedTask;
    }

    public Task SubscribeToTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
    public Task UnsubscribeFromTopicsAsync(IEnumerable<string> topics) => Task.CompletedTask;
}
