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
}
