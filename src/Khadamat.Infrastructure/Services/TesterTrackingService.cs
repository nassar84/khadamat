using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Identity;
using Khadamat.Infrastructure.Persistence;

namespace Khadamat.Infrastructure.Services;

public class TesterTrackingService : ITesterTrackingService
{
    private readonly KhadamatDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TesterTrackingService(KhadamatDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private static DateTime GetCairoToday()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }
        catch
        {
            return DateTime.UtcNow.AddHours(2).Date;
        }
    }

    public async Task<bool> IsTrackingEnabledAsync()
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Select(s => s.EnableTesterTracking)
            .FirstOrDefaultAsync();
        return settings;
    }

    public async Task<bool> SetTrackingEnabledAsync(bool enabled)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings { EnableTesterTracking = enabled };
            _context.AppSettings.Add(settings);
        }
        else
        {
            settings.EnableTesterTracking = enabled;
            settings.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TesterDeviceDto> RecordPingAsync(TesterPingRequest request, string? currentUserId, string? clientIp)
    {
        var isEnabled = await IsTrackingEnabledAsync();
        if (!isEnabled)
        {
            return new TesterDeviceDto
            {
                DeviceIdentifier = request.DeviceIdentifier ?? string.Empty,
                IsTrackingEnabled = false
            };
        }

        if (string.IsNullOrWhiteSpace(request.DeviceIdentifier))
        {
            request.DeviceIdentifier = Guid.NewGuid().ToString("N");
        }

        var cairoToday = GetCairoToday();

        var device = await _context.TesterDevices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == request.DeviceIdentifier);

        if (device == null)
        {
            device = new TesterDevice
            {
                DeviceIdentifier = request.DeviceIdentifier,
                UserId = currentUserId,
                TesterEmail = request.TesterEmail,
                TesterName = request.TesterName,
                DeviceModel = request.DeviceModel,
                OperatingSystem = request.OperatingSystem,
                IpAddress = clientIp,
                FirstSeenAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow,
                LastStreakDate = cairoToday,
                ConsecutiveDaysStreak = 1,
                TotalActiveDays = 1,
                TargetDays = 14,
                HasMissedStreak = false
            };
            _context.TesterDevices.Add(device);
        }
        else
        {
            device.LastSeenAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(clientIp)) device.IpAddress = clientIp;
            if (!string.IsNullOrEmpty(request.DeviceModel)) device.DeviceModel = request.DeviceModel;
            if (!string.IsNullOrEmpty(request.OperatingSystem)) device.OperatingSystem = request.OperatingSystem;
            if (!string.IsNullOrEmpty(currentUserId) && string.IsNullOrEmpty(device.UserId)) device.UserId = currentUserId;
            if (!string.IsNullOrEmpty(request.TesterEmail) && string.IsNullOrEmpty(device.TesterEmail)) device.TesterEmail = request.TesterEmail;
            if (!string.IsNullOrEmpty(request.TesterName) && string.IsNullOrEmpty(device.TesterName)) device.TesterName = request.TesterName;

            // Streak & attendance logic
            if (device.LastStreakDate == null)
            {
                device.LastStreakDate = cairoToday;
                device.ConsecutiveDaysStreak = 1;
                device.TotalActiveDays = 1;
            }
            else
            {
                var diffDays = (cairoToday - device.LastStreakDate.Value.Date).TotalDays;

                if (diffDays == 0)
                {
                    // Opened multiple times on the same calendar day: streak remains same
                }
                else if (diffDays == 1)
                {
                    // Consecutive day!
                    device.ConsecutiveDaysStreak += 1;
                    device.TotalActiveDays += 1;
                    device.LastStreakDate = cairoToday;
                }
                else if (diffDays > 1)
                {
                    // User missed at least one full day -> Streak resets to 1, but logs remain!
                    device.ConsecutiveDaysStreak = 1;
                    device.TotalActiveDays += 1;
                    device.HasMissedStreak = true;
                    device.LastStreakDate = cairoToday;
                }
            }
        }

        // If user logged in and profile info is missing, enrich from Identity
        var effectiveUserId = currentUserId ?? device.UserId;
        if (!string.IsNullOrEmpty(effectiveUserId))
        {
            device.UserId = effectiveUserId;
            if (string.IsNullOrEmpty(device.TesterEmail) || string.IsNullOrEmpty(device.TesterName))
            {
                var user = await _userManager.FindByIdAsync(effectiveUserId);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(device.TesterEmail)) device.TesterEmail = user.Email;
                    if (string.IsNullOrEmpty(device.TesterName)) device.TesterName = user.FullName;
                    if (string.IsNullOrEmpty(device.PhoneNumber)) device.PhoneNumber = user.PhoneNumber;
                }
            }
        }

        await _context.SaveChangesAsync();

        // Always record every single open visit into permanent activity log
        var log = new TesterActivityLog
        {
            TesterDeviceId = device.Id,
            DeviceIdentifier = device.DeviceIdentifier,
            UserId = device.UserId,
            TesterEmail = device.TesterEmail,
            OpenedAt = DateTime.UtcNow,
            VisitDate = cairoToday,
            IpAddress = clientIp,
            DeviceModel = device.DeviceModel,
            OperatingSystem = device.OperatingSystem,
            IsNativeApp = request.IsNativeApp
        };

        _context.TesterActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        return MapToDto(device, cairoToday);
    }

    public async Task<bool> IdentifyTesterAsync(TesterIdentifyRequest request, string? currentUserId)
    {
        var isEnabled = await IsTrackingEnabledAsync();
        if (!isEnabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.DeviceIdentifier)) return false;

        var device = await _context.TesterDevices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == request.DeviceIdentifier);

        if (device == null)
        {
            var cairoToday = GetCairoToday();
            device = new TesterDevice
            {
                DeviceIdentifier = request.DeviceIdentifier,
                TesterEmail = request.TesterEmail,
                TesterName = request.TesterName,
                PhoneNumber = request.PhoneNumber,
                UserId = currentUserId,
                FirstSeenAt = DateTime.UtcNow,
                LastSeenAt = DateTime.UtcNow,
                LastStreakDate = cairoToday,
                ConsecutiveDaysStreak = 1,
                TotalActiveDays = 1
            };
            _context.TesterDevices.Add(device);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.TesterEmail)) device.TesterEmail = request.TesterEmail.Trim();
            if (!string.IsNullOrWhiteSpace(request.TesterName)) device.TesterName = request.TesterName.Trim();
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) device.PhoneNumber = request.PhoneNumber.Trim();
            if (!string.IsNullOrWhiteSpace(currentUserId)) device.UserId = currentUserId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TestersDashboardSummaryDto> GetDashboardAsync()
    {
        var cairoToday = GetCairoToday();
        var isEnabled = await IsTrackingEnabledAsync();

        var devices = await _context.TesterDevices
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync();

        var dtoList = devices.Select(d => MapToDto(d, cairoToday, isEnabled)).ToList();

        var summary = new TestersDashboardSummaryDto
        {
            IsTrackingEnabled = isEnabled,
            TotalTesters = dtoList.Count,
            ActiveTodayCount = dtoList.Count(d => d.IsOpenToday),
            InactiveTodayCount = dtoList.Count(d => !d.IsOpenToday),
            CompletedTestersCount = dtoList.Count(d => d.IsCompleted),
            StreakBrokenCount = dtoList.Count(d => d.HasMissedStreak),
            Testers = dtoList
        };

        return summary;
    }

    public async Task<bool> AssignTesterAsync(int id, AssignTesterRequest request)
    {
        var device = await _context.TesterDevices.FindAsync(id);
        if (device == null) return false;

        if (request.TesterName != null) device.TesterName = request.TesterName.Trim();
        if (request.TesterEmail != null) device.TesterEmail = request.TesterEmail.Trim();
        if (request.PhoneNumber != null) device.PhoneNumber = request.PhoneNumber.Trim();
        if (request.Notes != null) device.Notes = request.Notes.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TesterActivityLogDto>> GetDeviceLogsAsync(int id)
    {
        return await _context.TesterActivityLogs
            .Where(l => l.TesterDeviceId == id)
            .OrderByDescending(l => l.OpenedAt)
            .Take(100)
            .Select(l => new TesterActivityLogDto
            {
                Id = l.Id,
                OpenedAt = l.OpenedAt,
                VisitDate = l.VisitDate,
                IpAddress = l.IpAddress,
                DeviceModel = l.DeviceModel,
                OperatingSystem = l.OperatingSystem,
                IsNativeApp = l.IsNativeApp
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteTesterDeviceAsync(int id)
    {
        var device = await _context.TesterDevices.FindAsync(id);
        if (device == null) return false;

        device.IsDeleted = true;
        device.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private static TesterDeviceDto MapToDto(TesterDevice d, DateTime cairoToday, bool isTrackingEnabled = true)
    {
        var isOpenToday = d.LastStreakDate.HasValue && d.LastStreakDate.Value.Date == cairoToday;

        return new TesterDeviceDto
        {
            Id = d.Id,
            DeviceIdentifier = d.DeviceIdentifier,
            UserId = d.UserId,
            TesterEmail = d.TesterEmail,
            TesterName = d.TesterName,
            PhoneNumber = d.PhoneNumber,
            DeviceModel = d.DeviceModel,
            OperatingSystem = d.OperatingSystem,
            IpAddress = d.IpAddress,
            FirstSeenAt = d.FirstSeenAt,
            LastSeenAt = d.LastSeenAt,
            LastStreakDate = d.LastStreakDate,
            ConsecutiveDaysStreak = d.ConsecutiveDaysStreak,
            TotalActiveDays = d.TotalActiveDays,
            TargetDays = d.TargetDays,
            HasMissedStreak = d.HasMissedStreak,
            Notes = d.Notes,
            IsOpenToday = isOpenToday,
            IsTrackingEnabled = isTrackingEnabled
        };
    }
}
