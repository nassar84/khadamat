using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/facebook")]
public class FacebookPublishController : ControllerBase
{
    private readonly IFacebookPublishService _publishService;

    public FacebookPublishController(IFacebookPublishService publishService)
    {
        _publishService = publishService;
    }

    [HttpGet("status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _publishService.GetIntegrationStatusAsync();
        return Ok(ApiResponse<FacebookIntegrationStatusDto>.Succeed(status));
    }

    [HttpGet("oauth-init")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InitiateOAuth([FromQuery] string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return BadRequest(ApiResponse<FacebookOAuthInitResponseDto>.Fail("رابط العودة مطلوب"));
        }

        var res = await _publishService.InitiateAdminAuthorizationAsync(returnUrl);
        return Ok(ApiResponse<FacebookOAuthInitResponseDto>.Succeed(res));
    }

    [HttpGet("oauth-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessOAuthCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(ApiResponse<List<FacebookManagedPageDto>>.Fail("بيانات التفويض غير مكتملة"));
        }

        var res = await _publishService.ProcessOAuthCallbackAsync(code, state);
        if (!res.Success)
        {
            return BadRequest(res);
        }

        return Ok(res);
    }

    [HttpPost("connect-page")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConnectPage([FromBody] FacebookConnectPageRequestDto request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "المسؤول";

        var res = await _publishService.ConnectPageAsync(request, adminId, adminName);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("disconnect-page")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DisconnectPage()
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var res = await _publishService.DisconnectPageAsync(adminId);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("test-connection")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestConnection()
    {
        var res = await _publishService.ValidatePageConnectionAsync();
        return Ok(ApiResponse<FacebookConnectionTestResultDto>.Succeed(res));
    }

    [HttpPost("test-permissions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestPermissions()
    {
        var res = await _publishService.ValidatePublishingPermissionsAsync();
        return Ok(ApiResponse<FacebookConnectionTestResultDto>.Succeed(res));
    }

    [HttpPost("publish/text")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishText([FromBody] FacebookPublishTextRequestDto request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminName = User.FindFirstValue(ClaimTypes.Name);

        var res = await _publishService.PublishTextPostAsync(request.Message, adminId, adminName);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("publish/photo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishPhoto([FromBody] FacebookPublishPhotoRequestDto request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminName = User.FindFirstValue(ClaimTypes.Name);

        var res = await _publishService.PublishPhotoPostAsync(request.Caption, request.PhotoUrl, adminId, adminName);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("publish/service-card")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishServiceCard([FromBody] FacebookPublishServiceCardRequestDto request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminName = User.FindFirstValue(ClaimTypes.Name);

        var res = await _publishService.PublishServiceCardAsync(request.ServiceId, request.CustomCaption, adminId, adminName);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpGet("preview-service-card/{serviceId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PreviewServiceCard(int serviceId)
    {
        var res = await _publishService.PrepareServiceCardPostTextAsync(serviceId);
        if (!res.Success) return NotFound(res);
        return Ok(res);
    }

    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLogs([FromQuery] int? serviceId = null, [FromQuery] string? status = null, [FromQuery] int take = 50)
    {
        var logs = await _publishService.GetPublishLogsAsync(serviceId, status, take);
        return Ok(ApiResponse<List<FacebookPublishLogDto>>.Succeed(logs));
    }

    [HttpPost("retry/{logId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Retry(int logId)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var res = await _publishService.RetryPublishAsync(logId, adminId);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("cancel/{logId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int logId)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
        var res = await _publishService.CancelPublishAsync(logId, adminId);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }

    [HttpPost("update-mode")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMode([FromBody] FacebookUpdateModeDto dto)
    {
        var res = await _publishService.UpdatePublishModeAsync(dto.PublishMode, dto.AutoPublishOnApproval);
        if (!res.Success) return BadRequest(res);
        return Ok(res);
    }
}
