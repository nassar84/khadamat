using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Khadamat.WebAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string receiverId, string message)
    {
        // In a real app, we persist to DB here or via Controller.
        // For SignalR, we just relay to the receiver.
        var senderId = Context.UserIdentifier;
        await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, message);
    }
}
