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
public class FavoritesController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public FavoritesController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<ServiceDto>>>> GetMyFavorites()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var favorites = await _context.Favorites
            .Include(f => f.Service)
            .ThenInclude(s => s.SubCategory)
            .Include(f => f.Service.City)
            .Where(f => f.UserId == userId && f.ServiceId != null)
            .Select(f => f.Service)
            .ToListAsync();

        // Convert to ServiceDto
        var dtos = favorites.Select(s => new ServiceDto
        {
            Id = s.Id,
            Title = s.Name,
            Description = s.Description,
            Price = s.Price,
            Images = string.IsNullOrEmpty(s.ImageUrl) ? new List<string>() : new List<string> { s.ImageUrl },
            SubCategoryName = s.SubCategory?.Name ?? "",
            CityName = s.City?.City_Name_AR ?? "",
            IsApproved = s.Approved,
            Rating = s.Ratings.Any() ? (double)s.Ratings.Average(r => r.Stars) : 0,
            IsFavorite = true
        }).ToList();

        return Ok(ApiResponse<List<ServiceDto>>.Succeed(dtos));
    }

    [HttpPost("toggle/{serviceId}")]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(int serviceId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == serviceId);

        if (existing != null)
        {
            _context.Favorites.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Succeed(false)); // Removed
        }
        else
        {
            _context.Favorites.Add(new Favorite(userId, serviceId: serviceId));
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Succeed(true)); // Added
        }
    }
}
