using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, string type = "System", string? link = null, string? imageUrl = null, string deliveryChannel = "Both");
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkMultipleAsReadAsync(string userId, List<int> notificationIds);
    Task MarkAllAsReadAsync(string userId);
    Task DeleteNotificationAsync(string userId, int notificationId);
    Task DeleteNotificationsAsync(string userId, List<int> notificationIds);
    Task DeleteAllNotificationsAsync(string userId);
    Task SendBroadcastAsync(string title, string message, string type, string? link, string? role, int? governorateId, int? cityId, int? mainCategoryId, string? imageUrl = null, string deliveryChannel = "Both");
}
