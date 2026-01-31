using Khadamat.Application.Interfaces;
using Khadamat.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Khadamat.WebAPI.Services;

public class SignalRNotificationNotifier : INotificationNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(string userId, string title, string message)
    {
        // "Recall: We add users to a Group named by their userId in OnConnectedAsync"
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", title, message);
    }
}
