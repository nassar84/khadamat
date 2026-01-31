using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly KhadamatDbContext _context;
    private readonly INotificationNotifier _notifier;

    public NotificationService(KhadamatDbContext context, INotificationNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string type = "System", string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedLink = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Push real-time
        await _notifier.NotifyUserAsync(userId, title, message);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                RelatedLink = n.RelatedLink,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var n = await _context.Notifications.FindAsync(notificationId);
        if (n != null)
        {
            n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Any())
        {
            foreach (var n in unread) n.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
    public async Task SendBroadcastAsync(string title, string message, string type, string? link, string? role, int? governorateId, int? cityId, int? mainCategoryId)
    {
        var usersQuery = _context.Users.AsQueryable();

        // Filter by Role
        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<Khadamat.Domain.Enums.UserRole>(role, true, out var roleEnum))
            {
                usersQuery = usersQuery.Where(u => u.Role == roleEnum);
            }
        }

        // Filter by Location
        if (cityId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.CityId == cityId);
        }
        else if (governorateId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.City!.GovernorateId == governorateId);
        }

        // Filter by Category (Only for Providers)
        if (mainCategoryId.HasValue)
        {
            usersQuery = usersQuery.Where(u => _context.Services.Any(s => s.ProviderProfile!.UserId == u.Id && s.Category!.MainCategoryId == mainCategoryId));
        }

        var targetUserIds = await usersQuery.Select(u => u.Id).ToListAsync();

        var notifications = targetUserIds.Select(userId => new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedLink = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        // Push real-time to each user
        foreach (var userId in targetUserIds)
        {
            await _notifier.NotifyUserAsync(userId, title, message);
        }
    }
}
