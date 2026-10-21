using System.Security.Claims;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("v1/notifications")]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IFcmPushService _fcmPushService;

    public NotificationsController(INotificationService notificationService, IFcmPushService fcmPushService)
    {
        _notificationService = notificationService;
        _fcmPushService = fcmPushService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirst("sub")?.Value 
                     ?? User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(ApiResponse<List<NotificationDto>>.Succeed(notifications));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirst("sub")?.Value 
                     ?? User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Succeed(count));
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirst("sub")?.Value 
                     ?? User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("send")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> SendNotification([FromBody] SendNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(ApiResponse<bool>.Fail("العنوان ونص الرسالة مطلوبان"));

        try
        {
            if (!string.IsNullOrEmpty(request.UserId))
            {
                await _notificationService.SendNotificationAsync(
                    request.UserId, request.Title, request.Message, "Admin", request.RelatedLink, request.ImageUrl);
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
                    request.MainCategoryId,
                    request.ImageUrl);
            }

            return Ok(ApiResponse<bool>.Succeed(true));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationsController] SendNotification error: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, ApiResponse<bool>.Fail($"فشل الإرسال: {ex.Message}"));
        }
    }

    [HttpPost("register-device")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> RegisterDevice([FromBody] RegisterDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Token))
            return BadRequest(ApiResponse<bool>.Fail("رمز الجهاز مطلوب"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? User.FindFirst("sub")?.Value 
                     ?? User.FindFirst("nameid")?.Value
                     ?? "";

        var success = await _fcmPushService.RegisterDeviceTokenAsync(
            userId, request.Token, request.Platform, request.DeviceId, request.AppVersion);

        return Ok(ApiResponse<bool>.Succeed(success));
    }

    [HttpDelete("unregister-device")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> UnregisterDevice([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(ApiResponse<bool>.Fail("رمز الجهاز مطلوب"));

        var success = await _fcmPushService.UnregisterDeviceTokenAsync(token);
        return Ok(ApiResponse<bool>.Succeed(success));
    }

    [HttpPost("test-fcm")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<string>>> TestFcm([FromQuery] string? title, [FromQuery] string? message, [FromQuery] string? imageUrl)
    {
        var testTitle = string.IsNullOrWhiteSpace(title) ? "إشعار تجريبي من خدماوى" : title;
        var testMessage = string.IsNullOrWhiteSpace(message) ? "هذا إشعار تجريبي لاختبار وصول التنبيهات الفورية عبر FCM بنجاح" : message;

        var count = await _fcmPushService.SendBroadcastPushAsync(testTitle, testMessage, "/", imageUrl);
        return Ok(ApiResponse<string>.Succeed($"تم تشغيل إشعار FCM التجريبي! تم التسليم إلى {count} جهاز مسجل."));
    }
}


