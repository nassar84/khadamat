using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Khadamat.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly KhadamatDbContext _context;
    private readonly INotificationNotifier _notifier;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationService(KhadamatDbContext context, INotificationNotifier notifier, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _notifier = notifier;
        _scopeFactory = scopeFactory;
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

        // Push real-time via SignalR
        await _notifier.NotifyUserAsync(userId, title, message);

        // Push to mobile devices via FCM in background (safe scoped execution)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var fcm = scope.ServiceProvider.GetRequiredService<IFcmPushService>();
                await fcm.SendPushToUserAsync(userId, title, message, link);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] FCM push error: {ex.Message}");
            }
        });
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

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
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
        try
        {
            var usersQuery = _context.Users.AsQueryable();

            // Filter by Role
            if (!string.IsNullOrEmpty(role))
            {
                if (Enum.TryParse<Khadamat.Domain.Enums.UserRole>(role, true, out var roleEnum))
                {
                    usersQuery = usersQuery.Where(u => u.Role == roleEnum);
                }
                // Support "Provider" as alias for users with IsProvider = true
                else if (string.Equals(role, "Provider", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = usersQuery.Where(u => u.IsProvider);
                }
            }

            // Filter by Location
            if (cityId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.CityId == cityId);
            }
            else if (governorateId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.City != null && u.City.GovernorateId == governorateId);
            }

            // Filter by Category (Only for Providers)
            if (mainCategoryId.HasValue)
            {
                usersQuery = usersQuery.Where(u => _context.Services.Any(s =>
                    s.ProviderProfile != null &&
                    s.ProviderProfile.UserId == u.Id &&
                    s.Category != null &&
                    s.Category.MainCategoryId == mainCategoryId));
            }

            var targetUserIds = await usersQuery.Select(u => u.Id).ToListAsync();

            Console.WriteLine($"[NotificationService] Broadcasting '{title}' to {targetUserIds.Count} users");

            if (targetUserIds.Count == 0)
            {
                Console.WriteLine("[NotificationService] No target users found for the given filters.");
                return;
            }

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

            // Push real-time via SignalR to each user
            foreach (var userId in targetUserIds)
            {
                await _notifier.NotifyUserAsync(userId, title, message);
            }

            // Push to mobile devices via FCM in background (safe scoped execution)
            bool isFilterApplied = !string.IsNullOrEmpty(role) || governorateId.HasValue || cityId.HasValue || mainCategoryId.HasValue;
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var fcm = scope.ServiceProvider.GetRequiredService<IFcmPushService>();
                    if (!isFilterApplied)
                    {
                        await fcm.SendBroadcastPushAsync(title, message, link);
                    }
                    else
                    {
                        await fcm.SendPushToUsersAsync(targetUserIds, title, message, link);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotificationService] FCM broadcast push error: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] SendBroadcastAsync error: {ex.Message}\n{ex.StackTrace}");
            throw; // Re-throw so controller can return 500 with details
        }
    }
}

