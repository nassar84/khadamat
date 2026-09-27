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
[Route("v1/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public ReviewsController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet("service/{serviceId}")]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetServiceReviews(int serviceId)
    {
        var ratings = await _context.Ratings
            .Where(r => r.ServiceId == serviceId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        var userIds = ratings.Select(r => r.UserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new {
                Name = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : (!string.IsNullOrWhiteSpace(u.UserName) ? u.UserName : "مستخدم"),
                Avatar = u.ProfileImageUrl
            });

        var reviews = ratings.Select(r => new ReviewDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = users.TryGetValue(r.UserId, out var u) ? u.Name : "مستخدم",
            UserAvatar = users.TryGetValue(r.UserId, out var u2) ? u2.Avatar : null,
            Rating = r.Stars,
            Comment = r.Comment,
            CreatedAt = r.Date
        }).ToList();

        return Ok(ApiResponse<List<ReviewDto>>.Succeed(reviews));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<MyReviewDto>>>> GetMyReviews()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var ratings = await _context.Ratings
            .Include(r => r.Service)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Date)
            .Select(r => new MyReviewDto
            {
                Id = r.Id,
                ServiceId = r.ServiceId,
                ServiceName = r.Service.Name,
                Rating = r.Stars,
                Comment = r.Comment,
                CreatedAt = r.Date
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MyReviewDto>>.Succeed(ratings));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(ApiResponse<ReviewResultDto>.Fail("التقييم يجب أن يكون من 1 إلى 5 نجوم"));

        // Check service exists
        var service = await _context.Services
            .Include(s => s.ProviderProfile)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);
        if (service == null)
            return NotFound(ApiResponse<ReviewResultDto>.Fail("الخدمة غير موجودة"));

        // Prevent provider from rating their own service
        if (service.ProviderProfile?.UserId == userId)
            return BadRequest(ApiResponse<ReviewResultDto>.Fail("لا يمكنك تقييم خدمتك الخاصة"));

        // One rating per user per service - check existing
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.ServiceId == request.ServiceId && r.UserId == userId);

        int reviewId = 0;
        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Stars   = request.Rating;
            existingRating.Comment = request.Comment ?? string.Empty;
            existingRating.Date    = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            reviewId = existingRating.Id;
        }
        else
        {
            // Create new rating (ServiceRequestId optional)
            var newRating = new Rating(
                request.ServiceId,
                userId,
                request.Rating,
                request.Comment ?? string.Empty,
                request.ServiceRequestId.HasValue && request.ServiceRequestId.Value > 0 ? request.ServiceRequestId : null
            );
            _context.Ratings.Add(newRating);
            await _context.SaveChangesAsync();
            reviewId = newRating.Id;
        }

        // Recalculate service average
        var allRatings = await _context.Ratings
            .Where(r => r.ServiceId == request.ServiceId)
            .ToListAsync();

        double newAvg   = allRatings.Any() ? allRatings.Average(r => r.Stars) : 0;
        int    newCount = allRatings.Count;

        // Get user display name from AspNetUsers
        var user = await _context.Users.FindAsync(userId);
        string userName = !string.IsNullOrWhiteSpace(user?.FullName)
            ? user.FullName
            : (!string.IsNullOrWhiteSpace(user?.UserName) ? user.UserName : "مستخدم");

        var result = new ReviewResultDto
        {
            ReviewId     = reviewId,
            NewAverage   = Math.Round(newAvg, 1),
            RatersCount  = newCount,
            ReviewerName = userName,
            ReviewerAvatar = user?.ProfileImageUrl
        };

        return Ok(ApiResponse<ReviewResultDto>.Succeed(result));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> EditReview(int id, [FromBody] EditReviewRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var review = await _context.Ratings.FindAsync(id);
        if (review == null) return NotFound();

        // Owner can edit their own; Admin can edit any
        if (review.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        if (request.Rating.HasValue)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(ApiResponse<bool>.Fail("التقييم يجب أن يكون من 1 إلى 5"));
            review.Stars = request.Rating.Value;
        }

        if (request.Comment != null)
            review.Comment = request.Comment;

        review.Date = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var review = await _context.Ratings.FindAsync(id);
        if (review == null) return NotFound();

        // Allow owner or Admin
        if (review.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        _context.Ratings.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    // ── Admin: list all reviews with filters ──────────────────────────────
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin,SystemAdmin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<AdminReviewDto>>>> AdminGetAllReviews(
        [FromQuery] int? serviceId, [FromQuery] int? stars, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var query = _context.Ratings.AsQueryable();
        if (serviceId.HasValue) query = query.Where(r => r.ServiceId == serviceId.Value);
        if (stars.HasValue)     query = query.Where(r => r.Stars == stars.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.Date)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new AdminReviewDto
            {
                Id         = r.Id,
                ServiceId  = r.ServiceId,
                UserId     = r.UserId,
                Stars      = r.Stars,
                Comment    = r.Comment,
                CreatedAt  = r.Date
            }).ToListAsync();

        // Enrich with names
        var uids = items.Select(i => i.UserId).Distinct().ToList();
        var sids = items.Select(i => i.ServiceId).Distinct().ToList();
        var users    = await _context.Users.Where(u => uids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.UserName ?? "مستخدم");
        var services = await _context.Services.Where(s => sids.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

        foreach (var item in items)
        {
            item.UserName    = users.TryGetValue(item.UserId, out var un) ? un : "مستخدم";
            item.ServiceName = services.TryGetValue(item.ServiceId, out var sn) ? sn : "خدمة";
        }

        return Ok(ApiResponse<List<AdminReviewDto>>.Succeed(items));
    }
}

public class EditReviewRequest
{
    public int?    Rating  { get; set; }
    public string? Comment { get; set; }
}

public class AdminReviewDto
{
    public int      Id          { get; set; }
    public int      ServiceId   { get; set; }
    public string   ServiceName { get; set; } = string.Empty;
    public string   UserId      { get; set; } = string.Empty;
    public string   UserName    { get; set; } = string.Empty;
    public int      Stars       { get; set; }
    public string   Comment     { get; set; } = string.Empty;
    public DateTime CreatedAt   { get; set; }
}

public class CreateReviewRequest
{
    public int ServiceId { get; set; }
    public int? ServiceRequestId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class MyReviewDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReviewResultDto
{
    public int ReviewId { get; set; }
    public double NewAverage { get; set; }
    public int RatersCount { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerAvatar { get; set; }
}
