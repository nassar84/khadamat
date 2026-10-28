using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;
using Khadamat.Domain.Enums;

namespace Khadamat.Application.Interfaces;

public interface IReminderService
{
    Task<List<ReminderCampaignDto>> GetRemindersAsync(ReminderFilterDto filter);
    Task<int> GetRemindersCountAsync(ReminderFilterDto filter);
    Task<ReminderCampaignDto?> GetReminderByIdAsync(int id);
    Task<ReminderCampaignDto> CreateReminderAsync(CreateReminderCampaignDto dto, string? userId);
    Task<ReminderCampaignDto?> UpdateReminderAsync(int id, UpdateReminderCampaignDto dto, string? userId);
    Task<bool> UpdateReminderStatusAsync(int id, ReminderStatus status, string? userId);
    Task<bool> DeleteReminderAsync(int id, string? userId);
    Task<ReminderCampaignDto?> DuplicateReminderAsync(int id, string? userId);
    
    /// <summary>
    /// Gets active reminders eligible for a client device
    /// </summary>
    Task<List<ActiveClientReminderDto>> GetActiveRemindersForClientAsync(string? userRole, bool isTester, string? language);
    
    /// <summary>
    /// Seeds or activates the default Google Play 14-day tester campaign
    /// </summary>
    Task<ReminderCampaignDto> EnsureDefaultTesterCampaignExistsAsync(string? userId);

    /// <summary>
    /// Immediately triggers a reminder campaign manually for testing and instant broadcast
    /// </summary>
    Task<(bool Success, string Message)> TriggerReminderNowAsync(int id, string? userId);

    // Reminder Bank Management
    Task<List<ReminderBankItemDto>> GetBankItemsAsync(string? category = null, bool? isActive = null);
    Task<ReminderBankItemDto?> GetBankItemByIdAsync(int id);
    Task<ReminderBankItemDto> CreateBankItemAsync(CreateReminderBankItemDto dto, string? userId);
    Task<ReminderBankItemDto?> UpdateBankItemAsync(int id, UpdateReminderBankItemDto dto, string? userId);
    Task<bool> DeleteBankItemAsync(int id, string? userId);
    Task<List<string>> GetBankCategoriesAsync();
    Task<ReminderBankItemDto?> ResolveNextBankItemAsync(int campaignId);
}
