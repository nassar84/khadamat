using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Domain.Entities;

namespace Khadamat.Application.Interfaces;

public interface IProviderEngagementService
{
    Task<EngagementKpiDto> GetEngagementKpisAsync();
    
    Task<List<ServiceEngagementSummaryDto>> GetEngageableServicesAsync(
        string? searchTerm = null,
        int? cityId = null,
        int? governorateId = null,
        string? profession = null,
        string? completenessFilter = null,
        string? contactStatus = null,
        string? missingField = null);

    ServiceCompletenessResultDto EvaluateServiceCompleteness(Service service);

    Task<PreparedEngagementMessageDto> PrepareEngagementMessageAsync(
        int serviceId,
        string reminderType,
        string channel = "WhatsApp",
        int? customCampaignId = null,
        string? customTitle = null,
        string? customCity = null,
        string? distinctionReason = null,
        bool? isDistinctionEnabled = null);

    Task<ApiResponse<int>> ExecuteEngagementAsync(
        SendEngagementRequestDto request,
        string adminUserId,
        string adminUserName);

    Task<ApiResponse<bool>> ConfirmWhatsAppSentAsync(int logId, string? notes = null);

    Task<ApiResponse<bool>> UpdateEngagementStatusAsync(int logId, string newStatus, string? notes = null);

    Task<ApiResponse<bool>> ToggleOptOutAsync(int serviceId, bool optOut, string? notes = null);

    Task<ServiceDistinctionConfigDto> GetDistinctionConfigAsync();
    Task<ApiResponse<bool>> SaveDistinctionConfigAsync(ServiceDistinctionConfigDto dto);
    Task<ApiResponse<bool>> SetServiceDistinctionAsync(int serviceId, string reason, bool isEnabled, string? notes = null);

    Task<List<MarketingCampaignDto>> GetCampaignsAsync();
    Task<ApiResponse<int>> SaveCampaignAsync(MarketingCampaignDto dto);
    Task<ApiResponse<bool>> DeleteCampaignAsync(int id);

    Task<List<ProfessionalTitleRuleDto>> GetProfessionalTitleRulesAsync();
    Task<ApiResponse<int>> SaveProfessionalTitleRuleAsync(ProfessionalTitleRuleDto dto);
    Task<ApiResponse<bool>> DeleteProfessionalTitleRuleAsync(int id);

    Task<List<ProviderEngagementReminderDto>> GetReminderTemplatesAsync();
    Task<ApiResponse<bool>> SaveReminderTemplateAsync(ProviderEngagementReminderDto dto);

    Task<List<EngagementLogDto>> GetEngagementLogsAsync(int? serviceId = null, int limit = 100);
}
