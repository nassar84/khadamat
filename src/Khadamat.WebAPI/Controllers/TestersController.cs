using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/testers")]
public class TestersController : ControllerBase
{
    private readonly ITesterTrackingService _trackingService;

    public TestersController(ITesterTrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? User.FindFirst("sub")?.Value
                                   ?? User.FindFirst("nameid")?.Value;

    private string? GetClientIp()
    {
        // Check X-Forwarded-For header from reverse proxies / Cloudflare
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }

        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.ToString().Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Silent background ping recorded whenever an app instance opens
    /// </summary>
    [HttpPost("ping")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<TesterDeviceDto>>> Ping([FromBody] TesterPingRequest request)
    {
        var clientIp = GetClientIp();
        var result = await _trackingService.RecordPingAsync(request, CurrentUserId, clientIp);
        return Ok(ApiResponse<TesterDeviceDto>.Succeed(result));
    }

    /// <summary>
    /// Allows a tester (even in guest mode) to bind their Google Play email to their device
    /// </summary>
    [HttpPost("identify")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> Identify([FromBody] TesterIdentifyRequest request)
    {
        var result = await _trackingService.IdentifyTesterAsync(request, CurrentUserId);
        return Ok(ApiResponse<bool>.Succeed(result));
    }

    /// <summary>
    /// Full dashboard for Google Play testers with attendance streak and status
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<TestersDashboardSummaryDto>>> GetDashboard()
    {
        var result = await _trackingService.GetDashboardAsync();
        return Ok(ApiResponse<TestersDashboardSummaryDto>.Succeed(result));
    }

    /// <summary>
    /// Admin assigns tester name, email, phone number, and notes to a device
    /// </summary>
    [HttpPut("{id:int}/assign")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignTester(int id, [FromBody] AssignTesterRequest request)
    {
        var result = await _trackingService.AssignTesterAsync(id, request);
        return Ok(ApiResponse<bool>.Succeed(result));
    }

    /// <summary>
    /// Detailed history of all visits for a specific tester device
    /// </summary>
    [HttpGet("{id:int}/logs")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<List<TesterActivityLogDto>>>> GetLogs(int id)
    {
        var result = await _trackingService.GetDeviceLogsAsync(id);
        return Ok(ApiResponse<List<TesterActivityLogDto>>.Succeed(result));
    }

    /// <summary>
    /// Delete/archive a tester device record
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _trackingService.DeleteTesterDeviceAsync(id);
        return Ok(ApiResponse<bool>.Succeed(result));
    }
}
