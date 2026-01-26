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
[Route("api/v1/[controller]")]
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

        // Basic validation: User shouldn't review own service? (Business rule)
        // User shouldn't review twice? 

        var rating = new Rating(request.ServiceId, userId, request.Rating, request.Comment);
        
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(rating.Id));
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
        {
            return Forbid();
        }

        _context.Ratings.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }
}

public class CreateReviewRequest
{
    public int ServiceId { get; set; }
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
