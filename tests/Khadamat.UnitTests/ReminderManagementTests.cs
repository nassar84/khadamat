using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.DTOs;
using Khadamat.Domain.Entities;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Infrastructure.Services;
using Khadamat.Application.Interfaces;
using Moq;
using Xunit;

namespace Khadamat.UnitTests;

public class ReminderManagementTests : IDisposable
{
    private readonly KhadamatDbContext _context;
    private readonly ReminderService _reminderService;

    public ReminderManagementTests()
    {
        var options = new DbContextOptionsBuilder<KhadamatDbContext>()
            .UseInMemoryDatabase(databaseName: "RemindersTestDb_" + Guid.NewGuid().ToString("N"))
            .Options;

        _context = new KhadamatDbContext(options);
        var mockNotificationService = new Mock<INotificationService>();
        _reminderService = new ReminderService(_context, mockNotificationService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateReminder_PersistsCorrectData()
    {
        // Arrange
        var createDto = new CreateReminderCampaignDto
        {
            Title = "تذكير يومي للمختبرين",
            InternalDescription = "اختبار مغلق",
            Message = "افتح التطبيق وجرب ميزة جديدة اليوم",
            Language = "ar",
            Status = ReminderStatus.Active,
            RecurrenceType = ReminderRecurrenceType.Daily,
            ScheduledTime = new TimeSpan(20, 0, 0),
            DurationDays = 14,
            StartDate = DateTime.UtcNow,
            TargetAudience = ReminderTargetAudience.Testers,
            DisplayType = ReminderDisplayType.NotificationBar,
            ActionRoute = "/services"
        };

        // Act
        var result = await _reminderService.CreateReminderAsync(createDto, "admin_user_1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("تذكير يومي للمختبرين", result.Title);
        Assert.Equal(new TimeSpan(20, 0, 0), result.ScheduledTime);
        Assert.Equal(ReminderStatus.Active, result.Status);
        Assert.Equal("/services", result.ActionRoute);
        Assert.Equal(14, result.DurationDays);
    }

    [Fact]
    public async Task UpdateReminder_ModifiesMessageAndTimeCorrectly()
    {
        // Arrange
        var createDto = new CreateReminderCampaignDto
        {
            Title = "تذكير أصلي",
            Message = "رسالة قديمة",
            ScheduledTime = new TimeSpan(18, 0, 0),
            Status = ReminderStatus.Active
        };
        var created = await _reminderService.CreateReminderAsync(createDto, "admin");

        var updateDto = new UpdateReminderCampaignDto
        {
            Id = created.Id,
            Title = "تذكير محدث",
            Message = "رسالة جديدة ومحدثة بالكامل",
            ScheduledTime = new TimeSpan(21, 30, 0),
            Status = ReminderStatus.Active,
            ActionRoute = "/profile"
        };

        // Act
        var updated = await _reminderService.UpdateReminderAsync(created.Id, updateDto, "admin_updater");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("تذكير محدث", updated.Title);
        Assert.Equal("رسالة جديدة ومحدثة بالكامل", updated.Message);
        Assert.Equal(new TimeSpan(21, 30, 0), updated.ScheduledTime);
        Assert.Equal("/profile", updated.ActionRoute);
    }

    [Fact]
    public async Task ToggleStatus_PausesAndReactivatesReminder()
    {
        // Arrange
        var createDto = new CreateReminderCampaignDto
        {
            Title = "تذكير قابل للإيقاف",
            Message = "رسالة التذكير",
            Status = ReminderStatus.Active
        };
        var created = await _reminderService.CreateReminderAsync(createDto, "admin");

        // Act 1: Pause
        var paused = await _reminderService.UpdateReminderStatusAsync(created.Id, ReminderStatus.Paused, "admin");
        var itemAfterPause = await _reminderService.GetReminderByIdAsync(created.Id);

        // Act 2: Reactivate
        var reactivated = await _reminderService.UpdateReminderStatusAsync(created.Id, ReminderStatus.Active, "admin");
        var itemAfterReactivation = await _reminderService.GetReminderByIdAsync(created.Id);

        // Assert
        Assert.True(paused);
        Assert.Equal(ReminderStatus.Paused, itemAfterPause?.Status);
        Assert.False(itemAfterPause?.IsActive);

        Assert.True(reactivated);
        Assert.Equal(ReminderStatus.Active, itemAfterReactivation?.Status);
        Assert.True(itemAfterReactivation?.IsActive);
    }

    [Fact]
    public async Task DuplicateReminder_CreatesDraftCloneWithoutOverwritingOriginal()
    {
        // Arrange
        var original = await _reminderService.CreateReminderAsync(new CreateReminderCampaignDto
        {
            Title = "تذكير الاستخدام",
            Message = "رسالة مهمة",
            ScheduledTime = new TimeSpan(20, 0, 0),
            Status = ReminderStatus.Active
        }, "admin");

        // Act
        var duplicate = await _reminderService.DuplicateReminderAsync(original.Id, "admin_cloner");

        // Assert
        Assert.NotNull(duplicate);
        Assert.NotEqual(original.Id, duplicate.Id);
        Assert.Contains("(نسخة)", duplicate.Title);
        Assert.Equal(ReminderStatus.Draft, duplicate.Status);
        Assert.False(duplicate.IsActive);
    }

    [Fact]
    public void IsCurrentlyActive_ReturnsFalse_WhenExpiredOrDeleted()
    {
        // Arrange: Expired campaign
        var expiredCampaign = new ReminderCampaign
        {
            Title = "حملة منتهية",
            Message = "...",
            Status = ReminderStatus.Active,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(-5),
            DurationDays = 14
        };

        var activeCampaign = new ReminderCampaign
        {
            Title = "حملة سارية",
            Message = "...",
            Status = ReminderStatus.Active,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(12),
            DurationDays = 14
        };

        // Assert
        Assert.False(expiredCampaign.IsCurrentlyActive(DateTime.UtcNow));
        Assert.True(activeCampaign.IsCurrentlyActive(DateTime.UtcNow));
    }

    [Fact]
    public async Task GetActiveRemindersForClient_FiltersByAudienceAndStatus()
    {
        // Arrange
        // 1. Tester campaign (Active)
        await _reminderService.CreateReminderAsync(new CreateReminderCampaignDto
        {
            Title = "للمختبرين فقط",
            Message = "رسالة المختبرين",
            TargetAudience = ReminderTargetAudience.Testers,
            Status = ReminderStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(13)
        }, "admin");

        // 2. Providers campaign (Active)
        await _reminderService.CreateReminderAsync(new CreateReminderCampaignDto
        {
            Title = "لمقدمي الخدمات فقط",
            Message = "أضف خدمة جديدة اليوم",
            TargetAudience = ReminderTargetAudience.Providers,
            Status = ReminderStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10)
        }, "admin");

        // 3. Paused campaign
        await _reminderService.CreateReminderAsync(new CreateReminderCampaignDto
        {
            Title = "تذكير متوقف",
            Message = "لن يظهر",
            TargetAudience = ReminderTargetAudience.AllUsers,
            Status = ReminderStatus.Paused
        }, "admin");

        // Act 1: Sync as Tester
        var testerReminders = await _reminderService.GetActiveRemindersForClientAsync(userRole: "User", isTester: true, language: "ar");

        // Act 2: Sync as Non-Tester Client
        var clientReminders = await _reminderService.GetActiveRemindersForClientAsync(userRole: "User", isTester: false, language: "ar");

        // Act 3: Sync as Provider
        var providerReminders = await _reminderService.GetActiveRemindersForClientAsync(userRole: "Provider", isTester: false, language: "ar");

        // Assert
        Assert.Contains(testerReminders, r => r.Title == "للمختبرين فقط");
        Assert.DoesNotContain(clientReminders, r => r.Title == "للمختبرين فقط");
        Assert.Contains(providerReminders, r => r.Title == "لمقدمي الخدمات فقط");
        Assert.DoesNotContain(testerReminders, r => r.Title == "تذكير متوقف");
    }

    [Fact]
    public async Task EnsureDefaultTesterCampaignExists_CreatesConfigurableTemplate()
    {
        // Act
        var result = await _reminderService.EnsureDefaultTesterCampaignExistsAsync("System");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("تذكير المختبرين بالاستخدام اليومي", result.Title);
        Assert.Equal(new TimeSpan(20, 0, 0), result.ScheduledTime); // 8:00 PM default
        Assert.Equal(14, result.DurationDays); // 14 days default
        Assert.Equal(ReminderRecurrenceType.Daily, result.RecurrenceType);
        Assert.Equal(ReminderTargetAudience.Testers, result.TargetAudience);
        Assert.Equal(ReminderDisplayType.NotificationBar, result.DisplayType);
        Assert.Equal("/", result.ActionRoute);
        Assert.Contains("مرحبًا بك في اختبار خدماوى", result.Message);
    }
}
