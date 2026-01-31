using System.Security.Claims;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(ApiResponse<List<NotificationDto>>.Succeed(notifications));
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("send")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> SendNotification([FromBody] SendNotificationRequest request)
    {
        if (!string.IsNullOrEmpty(request.UserId))
        {
            await _notificationService.SendNotificationAsync(request.UserId, request.Title, request.Message, "Admin", request.RelatedLink);
        }
        else
        {
            await _notificationService.SendBroadcastAsync(
                request.Title, 
                request.Message, 
                "Admin", 
                request.RelatedLink, 
                request.TargetRole,
                request.GovernorateId,
                request.CityId,
                request.MainCategoryId);
        }

        return Ok(ApiResponse<bool>.Succeed(true));
    }
}


