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
        // Join with Users to get reviewer name/avatar
        // Note: Cross-context join (Identity + App) might be tricky if not in same DB context. 
        // Assuming Identity is same DB or we fetch separately. 
        // For simplicity here, Assuming monolithic context or ignore user details fetch optimization.
        
        var ratings = await _context.Ratings
            .Where(r => r.ServiceId == serviceId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();

        // In a real app we'd fetch user details here. For now returning IDs or placeholders is acceptable if tight on time.
        // Or we can assume we only need the review content.
        
        var reviews = ratings.Select(r => new ReviewDto
        {
            Id = r.Id,
            UserName = "User", // Placeholder, requires Identity fetch
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

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Stars   = request.Rating;
            existingRating.Comment = request.Comment ?? string.Empty;
            existingRating.Date    = DateTime.UtcNow;
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
        }

        await _context.SaveChangesAsync();

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
            : (user?.UserName ?? "مستخدم");

        var result = new ReviewResultDto
        {
            ReviewId    = existingRating?.Id ?? 0,
            NewAverage  = Math.Round(newAvg, 1),
            RatersCount = newCount,
            ReviewerName = userName
        };

        return Ok(ApiResponse<ReviewResultDto>.Succeed(result));
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
}
