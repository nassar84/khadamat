using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Enums;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/reminders")]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;

    public RemindersController(IReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? User.FindFirst("sub")?.Value
                                   ?? User.FindFirst("nameid")?.Value;

    /// <summary>
    /// Gets list of reminders for admin management
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<List<ReminderCampaignDto>>>> GetAll([FromQuery] ReminderFilterDto filter)
    {
        var items = await _reminderService.GetRemindersAsync(filter);
        var total = await _reminderService.GetRemindersCountAsync(filter);
        Response.Headers["X-Total-Count"] = total.ToString();

        return Ok(ApiResponse<List<ReminderCampaignDto>>.Succeed(items));
    }

    /// <summary>
    /// Gets single reminder details
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderCampaignDto>>> GetById(int id)
    {
        var item = await _reminderService.GetReminderByIdAsync(id);
        if (item == null)
            return NotFound(ApiResponse<ReminderCampaignDto>.Fail("التذكير غير موجود"));

        return Ok(ApiResponse<ReminderCampaignDto>.Succeed(item));
    }

    /// <summary>
    /// Creates a new reminder campaign
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderCampaignDto>>> Create([FromBody] CreateReminderCampaignDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(ApiResponse<ReminderCampaignDto>.Fail("عنوان التذكير ونص الرسالة مطلوبان"));
        }

        var result = await _reminderService.CreateReminderAsync(dto, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ReminderCampaignDto>.Succeed(result));
    }

    /// <summary>
    /// Updates an existing reminder campaign
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderCampaignDto>>> Update(int id, [FromBody] UpdateReminderCampaignDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(ApiResponse<ReminderCampaignDto>.Fail("عنوان التذكير ونص الرسالة مطلوبان"));
        }

        var result = await _reminderService.UpdateReminderAsync(id, dto, CurrentUserId);
        if (result == null)
            return NotFound(ApiResponse<ReminderCampaignDto>.Fail("التذكير غير موجود"));

        return Ok(ApiResponse<ReminderCampaignDto>.Succeed(result));
    }

    /// <summary>
    /// Updates status of reminder (Draft, Active, Paused, Expired)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(int id, [FromBody] UpdateReminderStatusRequest request)
    {
        var success = await _reminderService.UpdateReminderStatusAsync(id, request.Status, CurrentUserId);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("التذكير غير موجود"));

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    /// <summary>
    /// Soft deletes a reminder
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var success = await _reminderService.DeleteReminderAsync(id, CurrentUserId);
        if (!success)
            return NotFound(ApiResponse<bool>.Fail("التذكير غير موجود"));

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    /// <summary>
    /// Duplicates an existing reminder to create a draft clone
    /// </summary>
    [HttpPost("{id:int}/duplicate")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderCampaignDto>>> Duplicate(int id)
    {
        var duplicate = await _reminderService.DuplicateReminderAsync(id, CurrentUserId);
        if (duplicate == null)
            return NotFound(ApiResponse<ReminderCampaignDto>.Fail("التذكير الأصلي غير موجود"));

        return Ok(ApiResponse<ReminderCampaignDto>.Succeed(duplicate));
    }

    /// <summary>
    /// Seeds or restores the default 14-day Google Play tester campaign
    /// </summary>
    [HttpPost("seed-default")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderCampaignDto>>> SeedDefault()
    {
        var result = await _reminderService.EnsureDefaultTesterCampaignExistsAsync(CurrentUserId);
        return Ok(ApiResponse<ReminderCampaignDto>.Succeed(result));
    }

    /// <summary>
    /// Immediately triggers this reminder for testing and validation
    /// </summary>
    [HttpPost("{id:int}/test-trigger")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<string>>> TestTrigger(int id)
    {
        var (success, message) = await _reminderService.TriggerReminderNowAsync(id, CurrentUserId);
        if (!success)
            return BadRequest(ApiResponse<string>.Fail(message));

        return Ok(ApiResponse<string>.Succeed(message));
    }

    /// <summary>
    /// Client synchronization endpoint: returns active reminders for the mobile app
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ActiveClientReminderDto>>>> GetActiveForClient(
        [FromQuery] string? role = null,
        [FromQuery] bool isTester = true,
        [FromQuery] string lang = "ar")
    {
        // If user is authenticated, derive role from claims if not supplied
        if (string.IsNullOrEmpty(role) && User.Identity?.IsAuthenticated == true)
        {
            role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirst("role")?.Value;
        }

        var activeReminders = await _reminderService.GetActiveRemindersForClientAsync(role, isTester, lang);
        return Ok(ApiResponse<List<ActiveClientReminderDto>>.Succeed(activeReminders));
    }

    #region Reminder Bank Endpoints

    /// <summary>
    /// Gets all reminder bank items with optional category and isActive filters
    /// </summary>
    [HttpGet("bank")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<List<ReminderBankItemDto>>>> GetBankItems(
        [FromQuery] string? category = null, 
        [FromQuery] bool? isActive = null)
    {
        var items = await _reminderService.GetBankItemsAsync(category, isActive);
        return Ok(ApiResponse<List<ReminderBankItemDto>>.Succeed(items));
    }

    /// <summary>
    /// Gets all unique bank categories
    /// </summary>
    [HttpGet("bank/categories")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetBankCategories()
    {
        var categories = await _reminderService.GetBankCategoriesAsync();
        return Ok(ApiResponse<List<string>>.Succeed(categories));
    }

    /// <summary>
    /// Gets a single bank item by id
    /// </summary>
    [HttpGet("bank/{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderBankItemDto>>> GetBankItemById(int id)
    {
        var item = await _reminderService.GetBankItemByIdAsync(id);
        if (item == null)
            return NotFound(ApiResponse<ReminderBankItemDto>.Fail("عنصر بنك التذكيرات غير موجود"));

        return Ok(ApiResponse<ReminderBankItemDto>.Succeed(item));
    }

    /// <summary>
    /// Creates a new bank item
    /// </summary>
    [HttpPost("bank")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderBankItemDto>>> CreateBankItem([FromBody] CreateReminderBankItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(ApiResponse<ReminderBankItemDto>.Fail("العنوان ونص التذكير مطلوبان"));

        var created = await _reminderService.CreateBankItemAsync(dto, CurrentUserId);
        return Ok(ApiResponse<ReminderBankItemDto>.Succeed(created, "تم إضافة التذكير إلى البنك بنجاح"));
    }

    /// <summary>
    /// Updates an existing bank item
    /// </summary>
    [HttpPut("bank/{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderBankItemDto>>> UpdateBankItem(int id, [FromBody] UpdateReminderBankItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(ApiResponse<ReminderBankItemDto>.Fail("العنوان ونص التذكير مطلوبان"));

        var updated = await _reminderService.UpdateBankItemAsync(id, dto, CurrentUserId);
        if (updated == null)
            return NotFound(ApiResponse<ReminderBankItemDto>.Fail("عنصر بنك التذكيرات غير موجود"));

        return Ok(ApiResponse<ReminderBankItemDto>.Succeed(updated, "تم تحديث عنصر بنك التذكيرات بنجاح"));
    }

    /// <summary>
    /// Deletes a bank item
    /// </summary>
    [HttpDelete("bank/{id:int}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBankItem(int id)
    {
        var deleted = await _reminderService.DeleteBankItemAsync(id, CurrentUserId);
        if (!deleted)
            return NotFound(ApiResponse<bool>.Fail("عنصر بنك التذكيرات غير موجود"));

        return Ok(ApiResponse<bool>.Succeed(true, "تم حذف التذكير من البنك بنجاح"));
    }

    /// <summary>
    /// Resolves the next bank item for a campaign
    /// </summary>
    [HttpGet("{id:int}/next-bank-item")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<ActionResult<ApiResponse<ReminderBankItemDto>>> GetNextBankItem(int id)
    {
        var nextItem = await _reminderService.ResolveNextBankItemAsync(id);
        if (nextItem == null)
            return NotFound(ApiResponse<ReminderBankItemDto>.Fail("لا توجد رسائل متاحة في البنك لهذه الحملة"));

        return Ok(ApiResponse<ReminderBankItemDto>.Succeed(nextItem));
    }

    #endregion
}
