using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/engagement")]
[Authorize(Roles = "SuperAdmin,SystemAdmin,Admin")]
public class ProviderEngagementController : ControllerBase
{
    private readonly IProviderEngagementService _engagementService;

    public ProviderEngagementController(IProviderEngagementService engagementService)
    {
        _engagementService = engagementService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var result = await _engagementService.GetEngagementKpisAsync();
        return Ok(ApiResponse<EngagementKpiDto>.Succeed(result));
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices(
        [FromQuery] string? search = null,
        [FromQuery] int? cityId = null,
        [FromQuery] int? governorateId = null,
        [FromQuery] string? profession = null,
        [FromQuery] string? completeness = null,
        [FromQuery] string? contactStatus = null,
        [FromQuery] string? missingField = null)
    {
        var result = await _engagementService.GetEngageableServicesAsync(
            search, cityId, governorateId, profession, completeness, contactStatus, missingField);
        return Ok(ApiResponse<System.Collections.Generic.List<ServiceEngagementSummaryDto>>.Succeed(result));
    }

    [HttpGet("prepare")]
    public async Task<IActionResult> PrepareMessage(
        [FromQuery] int serviceId,
        [FromQuery] string reminderType = "WelcomeAfterRegistration",
        [FromQuery] string channel = "WhatsApp",
        [FromQuery] int? campaignId = null,
        [FromQuery] string? title = null,
        [FromQuery] string? city = null,
        [FromQuery] string? distinctionReason = null,
        [FromQuery] bool? enableDistinction = null)
    {
        try
        {
            var result = await _engagementService.PrepareEngagementMessageAsync(
                serviceId, reminderType, channel, campaignId, title, city, distinctionReason, enableDistinction);
            return Ok(ApiResponse<PreparedEngagementMessageDto>.Succeed(result));
        }
        catch (System.Collections.Generic.KeyNotFoundException knf)
        {
            return NotFound(ApiResponse<PreparedEngagementMessageDto>.Fail(knf.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PreparedEngagementMessageDto>.Fail(ex.Message));
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEngagement([FromBody] SendEngagementRequestDto request)
    {
        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "admin";
        var adminUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "المسؤول";

        var result = await _engagementService.ExecuteEngagementAsync(request, adminUserId, adminUserName);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("confirm-whatsapp/{logId}")]
    public async Task<IActionResult> ConfirmWhatsAppSent(int logId, [FromQuery] string? notes = null)
    {
        var result = await _engagementService.ConfirmWhatsAppSentAsync(logId, notes);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("status/{logId}")]
    public async Task<IActionResult> UpdateStatus(int logId, [FromQuery] string newStatus, [FromQuery] string? notes = null)
    {
        var result = await _engagementService.UpdateEngagementStatusAsync(logId, newStatus, notes);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("opt-out/{serviceId}")]
    public async Task<IActionResult> ToggleOptOut(int serviceId, [FromQuery] bool optOut, [FromQuery] string? notes = null)
    {
        var result = await _engagementService.ToggleOptOutAsync(serviceId, optOut, notes);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns()
    {
        var result = await _engagementService.GetCampaignsAsync();
        return Ok(ApiResponse<System.Collections.Generic.List<MarketingCampaignDto>>.Succeed(result));
    }

    [HttpPost("campaigns")]
    public async Task<IActionResult> SaveCampaign([FromBody] MarketingCampaignDto dto)
    {
        var result = await _engagementService.SaveCampaignAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("campaigns/{id}")]
    public async Task<IActionResult> DeleteCampaign(int id)
    {
        var result = await _engagementService.DeleteCampaignAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("titles")]
    public async Task<IActionResult> GetTitles()
    {
        var result = await _engagementService.GetProfessionalTitleRulesAsync();
        return Ok(ApiResponse<System.Collections.Generic.List<ProfessionalTitleRuleDto>>.Succeed(result));
    }

    [HttpPost("titles")]
    public async Task<IActionResult> SaveTitle([FromBody] ProfessionalTitleRuleDto dto)
    {
        var result = await _engagementService.SaveProfessionalTitleRuleAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("titles/{id}")]
    public async Task<IActionResult> DeleteTitle(int id)
    {
        var result = await _engagementService.DeleteProfessionalTitleRuleAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var result = await _engagementService.GetReminderTemplatesAsync();
        return Ok(ApiResponse<System.Collections.Generic.List<ProviderEngagementReminderDto>>.Succeed(result));
    }

    [HttpPost("templates")]
    public async Task<IActionResult> SaveTemplate([FromBody] ProviderEngagementReminderDto dto)
    {
        var result = await _engagementService.SaveReminderTemplateAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] int? serviceId = null, [FromQuery] int limit = 100)
    {
        var result = await _engagementService.GetEngagementLogsAsync(serviceId, limit);
        return Ok(ApiResponse<System.Collections.Generic.List<EngagementLogDto>>.Succeed(result));
    }

    [HttpGet("distinction-config")]
    public async Task<IActionResult> GetDistinctionConfig()
    {
        var result = await _engagementService.GetDistinctionConfigAsync();
        return Ok(ApiResponse<ServiceDistinctionConfigDto>.Succeed(result));
    }

    [HttpPost("distinction-config")]
    public async Task<IActionResult> SaveDistinctionConfig([FromBody] ServiceDistinctionConfigDto dto)
    {
        var result = await _engagementService.SaveDistinctionConfigAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("service-distinction/{serviceId}")]
    public async Task<IActionResult> SetServiceDistinction(
        int serviceId,
        [FromQuery] string reason,
        [FromQuery] bool isEnabled,
        [FromQuery] string? notes = null)
    {
        var result = await _engagementService.SetServiceDistinctionAsync(serviceId, reason, isEnabled, notes);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
