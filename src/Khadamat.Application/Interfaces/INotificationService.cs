using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, string type = "System", string? link = null);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task SendBroadcastAsync(string title, string message, string type, string? link, string? role, int? governorateId, int? cityId, int? mainCategoryId);
}
