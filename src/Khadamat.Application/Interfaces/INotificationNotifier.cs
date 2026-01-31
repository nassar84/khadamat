namespace Khadamat.Application.Interfaces;

public interface INotificationNotifier
{
    Task NotifyUserAsync(string userId, string title, string message);
}
