using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public SubscriptionsController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _context.SubscriptionPlans
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DurationInDays = p.DurationInDays,
                MaxServices = p.MaxServices,
                IsFeatured = p.IsFeatured
            })
            .ToListAsync();

        return Ok(plans);
    }

    [Authorize]
    [HttpGet("my-subscription")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var provider = await _context.ProviderProfiles
            .Include(p => p.Subscription)
            .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (provider?.Subscription == null) return NotFound("No active subscription found.");

        var dto = new ProviderSubscriptionDto
        {
            Id = provider.Subscription.Id,
            PlanId = provider.Subscription.PlanId,
            PlanName = provider.Subscription.Plan.Name,
            StartDate = provider.Subscription.StartDate,
            EndDate = provider.Subscription.EndDate,
            IsActive = provider.Subscription.IsActive
        };

        return Ok(dto);
    }

    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var provider = await _context.ProviderProfiles
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (provider == null) return BadRequest("Only providers can subscribe to plans.");

        var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
        if (plan == null) return NotFound("Subscription plan not found.");

        // In a real app, you'd verify payment here. 
        // For now, we'll just create the subscription.

        // Deactivate old subscription if exists
        var oldSub = await _context.ProviderSubscriptions.FirstOrDefaultAsync(s => s.ProviderId == provider.Id && s.IsActive);
        if (oldSub != null)
        {
            oldSub.Cancel();
        }

        var newSub = new ProviderSubscription(provider.Id, plan.Id, plan.DurationInDays);
        _context.ProviderSubscriptions.Add(newSub);
        
        await _context.SaveChangesAsync();

        // Link to provider profile
        provider.SubscriptionId = newSub.Id;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true, "Subscribed successfully"));
    }

    // ── Admin: Plan Management ──────────────────────────────────────────

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] SubscriptionPlanDto dto)
    {
        var plan = new SubscriptionPlan(dto.Name, dto.Price, dto.DurationInDays, dto.MaxServices, dto.IsFeatured);
        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
        return Ok(true);
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("plans/{id}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] SubscriptionPlanDto dto)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null) return NotFound();

        // SubscriptionPlan properties have private setters, we can recreate or reflect/update
        _context.SubscriptionPlans.Remove(plan);
        var updated = new SubscriptionPlan(dto.Name, dto.Price, dto.DurationInDays, dto.MaxServices, dto.IsFeatured);
        _context.SubscriptionPlans.Add(updated);
        await _context.SaveChangesAsync();
        return Ok(true);
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("plans/{id}/delete")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null) return NotFound();

        // Check if any subscriptions use this plan
        bool inUse = await _context.ProviderSubscriptions.AnyAsync(s => s.PlanId == id);
        if (inUse)
        {
            return BadRequest("لا يمكن حذف الخطة لوجود اشتراكات مرتبطة بها.");
        }

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();
        return Ok(true);
    }

    // ── Admin: Provider Subscriptions Management ────────────────────────

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpGet("admin/providers")]
    public async Task<IActionResult> GetAdminProviders([FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var query = _context.ProviderProfiles
            .Include(p => p.Subscription)
            .ThenInclude(s => s!.Plan)
            .Include(p => p.City)
            .Include(p => p.Services)
            .AsNoTracking()
            .AsQueryable();

        var providers = await query.ToListAsync();

        // Fetch User accounts
        var userIds = providers.Select(p => p.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        var list = providers.Select(p =>
        {
            users.TryGetValue(p.UserId, out var user);
            var sub = p.Subscription;
            var plan = sub?.Plan;

            return new AdminProviderSubscriptionDto
            {
                ProviderId = p.Id,
                UserId = p.UserId,
                BusinessName = !string.IsNullOrEmpty(p.BusinessName) ? p.BusinessName : (user?.FullName ?? "بدون اسم"),
                OwnerName = user?.FullName ?? p.BusinessName,
                Email = user?.Email ?? "",
                PhoneNumber = !string.IsNullOrEmpty(p.ContactNumber) ? p.ContactNumber : (user?.PhoneNumber ?? ""),
                CityName = p.City?.City_Name_AR,
                Photo = p.Photo,
                SubscriptionId = sub?.Id,
                PlanId = sub?.PlanId,
                PlanName = plan?.Name ?? (sub != null ? "الخطة المجانية" : "غير مشترك"),
                StartDate = sub?.StartDate,
                EndDate = sub?.EndDate,
                IsActive = sub?.IsActive ?? false,
                ServicesCount = p.Services?.Count ?? 0,
                MaxServices = plan?.MaxServices ?? 5
            };
        }).ToList();

        // Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLower();
            list = list.Where(x => 
                (x.BusinessName?.ToLower().Contains(s) ?? false) ||
                (x.OwnerName?.ToLower().Contains(s) ?? false) ||
                (x.Email?.ToLower().Contains(s) ?? false) ||
                (x.PhoneNumber?.Contains(s) ?? false)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                list = list.Where(x => x.IsActive && !x.IsExpired).ToList();
            else if (status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                list = list.Where(x => x.IsExpired || !x.IsActive).ToList();
            else if (status.Equals("expiring", StringComparison.OrdinalIgnoreCase))
                list = list.Where(x => x.IsActive && !x.IsExpired && x.DaysRemainingCount <= 7).ToList();
        }

        return Ok(list.OrderBy(x => x.DaysRemainingCount).ToList());
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("admin/extend")]
    public async Task<IActionResult> ExtendSubscriptions([FromBody] ExtendSubscriptionsRequest request)
    {
        if (request.DaysToAdd <= 0) request.DaysToAdd = 90;

        // Default Free Plan fallback
        var defaultPlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Price == 0)
            ?? await _context.SubscriptionPlans.FirstOrDefaultAsync();

        var query = _context.ProviderProfiles
            .Include(p => p.Subscription)
            .AsQueryable();

        if (!request.All && request.ProviderIds != null && request.ProviderIds.Any())
        {
            query = query.Where(p => request.ProviderIds.Contains(p.Id));
        }

        var providers = await query.ToListAsync();
        int affected = 0;

        foreach (var p in providers)
        {
            if (p.Subscription != null)
            {
                p.Subscription.Extend(request.DaysToAdd);
                affected++;
            }
            else if (defaultPlan != null)
            {
                var newSub = new ProviderSubscription(p.Id, defaultPlan.Id, request.DaysToAdd);
                _context.ProviderSubscriptions.Add(newSub);
                await _context.SaveChangesAsync();
                p.SubscriptionId = newSub.Id;
                affected++;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<int>.Succeed(affected, $"تم تمديد الاشتراك بنجاح لـ {affected} مقدم خدمة بمقدار {request.DaysToAdd} يوماً."));
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("admin/toggle-status")]
    public async Task<IActionResult> ToggleSubscriptionsStatus([FromBody] ToggleSubscriptionsStatusRequest request)
    {
        var query = _context.ProviderProfiles
            .Include(p => p.Subscription)
            .AsQueryable();

        if (!request.All && request.ProviderIds != null && request.ProviderIds.Any())
        {
            query = query.Where(p => request.ProviderIds.Contains(p.Id));
        }

        var providers = await query.ToListAsync();
        int affected = 0;

        foreach (var p in providers)
        {
            if (p.Subscription != null)
            {
                p.Subscription.SetStatus(request.IsActive);
                affected++;
            }
        }

        await _context.SaveChangesAsync();
        string actionText = request.IsActive ? "تفعيل" : "إيقاف";
        return Ok(ApiResponse<int>.Succeed(affected, $"تم {actionText} الاشتراك لـ {affected} مقدم خدمة بنجاح."));
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost("admin/assign-plan")]
    public async Task<IActionResult> AssignPlan([FromBody] AssignPlanRequest request)
    {
        var provider = await _context.ProviderProfiles
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId);

        if (provider == null) return NotFound("مقدم الخدمة غير موجود.");

        var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
        if (plan == null) return NotFound("خطة الاشتراك غير موجودة.");

        int duration = request.CustomDays.HasValue && request.CustomDays.Value > 0 
            ? request.CustomDays.Value 
            : (plan.DurationInDays > 0 ? plan.DurationInDays : 90);

        if (provider.Subscription != null)
        {
            provider.Subscription.ChangePlan(plan.Id, duration);
        }
        else
        {
            var newSub = new ProviderSubscription(provider.Id, plan.Id, duration);
            _context.ProviderSubscriptions.Add(newSub);
            await _context.SaveChangesAsync();
            provider.SubscriptionId = newSub.Id;
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true, $"تم تعيين خطة \"{plan.Name}\" لمقدم الخدمة بنجاح."));
    }
}

