using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Khadamat.Domain.Entities;
using Khadamat.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdsController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public AdsController(KhadamatDbContext context)
    {
        _context = context;
    }

    // Public Endpoint: Get active ads for slider or specific placement
    [HttpGet("placements/{placement}")]
    public async Task<IActionResult> GetAdsByPlacement(string placement)
    {
        var now = DateTime.UtcNow;
        var ads = await _context.Ads
            .Where(a => !a.IsDeleted && a.Approved && a.Placement == placement && 
                        a.StartDate <= now && a.EndDate >= now)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new EnhancedAdDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                AdType = a.AdType,
                ImageUrl = a.ImagePath,
                VideoUrl = a.VideoUrl,
                TextContent = a.TextContent,
                TargetUrl = a.RedirectUrl,
                TargetCategories = a.CategoryID.HasValue ? a.CategoryID.ToString() : null,
                TargetKeywords = a.TargetKeywords,
                Placement = a.Placement,
                DisplayOrder = a.DisplayOrder,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                IsActive = true,
                ViewCount = a.Views,
                ClickCount = a.Clicks
            })
            .ToListAsync();
        
        return Ok(ApiResponse<List<EnhancedAdDto>>.Succeed(ads));
    }

    [HttpGet("slider")]
    public async Task<IActionResult> GetSliderAds()
    {
        return await GetAdsByPlacement("Slider");
    }

    // Admin APIs
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetAllAds()
    {
        var ads = await _context.Ads
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new EnhancedAdDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                AdType = a.AdType,
                ImageUrl = a.ImagePath,
                VideoUrl = a.VideoUrl,
                TextContent = a.TextContent,
                TargetUrl = a.RedirectUrl,
                TargetCategories = a.CategoryID.HasValue ? a.CategoryID.ToString() : null,
                TargetKeywords = a.TargetKeywords,
                Placement = a.Placement,
                DisplayOrder = a.DisplayOrder,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                IsActive = a.Approved,
                ViewCount = a.Views,
                ClickCount = a.Clicks,
                CreatedAt = a.CreatedAt,
                TargetGovernorates = a.TargetGovernorates,
                TargetCities = a.TargetCities,
                TargetServices = a.TargetServices,
                TargetUserGender = a.TargetUserGender,
                TargetDays = a.TargetDays,
                TargetMonths = a.TargetMonths,
                TargetTimeStart = a.TargetTimeStart,
                TargetTimeEnd = a.TargetTimeEnd
            })
            .ToListAsync();

        return Ok(ApiResponse<List<EnhancedAdDto>>.Succeed(ads));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetAdById(int id)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null || ad.IsDeleted) return NotFound();

        var dto = new EnhancedAdDto
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            AdType = ad.AdType,
            ImageUrl = ad.ImagePath,
            VideoUrl = ad.VideoUrl,
            TextContent = ad.TextContent,
            TargetUrl = ad.RedirectUrl,
            Placement = ad.Placement,
            DisplayOrder = ad.DisplayOrder,
            StartDate = ad.StartDate,
            EndDate = ad.EndDate,
            IsActive = ad.Approved,
            TargetKeywords = ad.TargetKeywords,
            ViewCount = ad.Views,
            ClickCount = ad.Clicks,
            CreatedAt = ad.CreatedAt,
            TargetGovernorates = ad.TargetGovernorates,
            TargetCities = ad.TargetCities,
            TargetServices = ad.TargetServices,
            TargetUserGender = ad.TargetUserGender,
            TargetDays = ad.TargetDays,
            TargetMonths = ad.TargetMonths,
            TargetTimeStart = ad.TargetTimeStart,
            TargetTimeEnd = ad.TargetTimeEnd
        };

        return Ok(ApiResponse<EnhancedAdDto>.Succeed(dto));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> CreateAd([FromBody] EnhancedAdDto dto)
    {
        // Parse category ID if simple single selection, else extend logic
        int? categoryId = null;
        if (int.TryParse(dto.TargetCategories, out int cid)) categoryId = cid;

        var ad = new Ad(
            dto.Title, 
            dto.Description ?? "", 
            dto.StartDate ?? DateTime.UtcNow, 
            dto.EndDate ?? DateTime.UtcNow.AddMonths(1),
            dto.AdType ?? "Image",
            null, // ActivityId
            categoryId
        );

        ad.UpdateDetails(
            dto.Title,
            dto.Description ?? "",
            dto.StartDate ?? DateTime.UtcNow,
            dto.EndDate ?? DateTime.UtcNow.AddMonths(1),
            dto.TargetUrl,
            dto.Placement,
            null, // City
            null, // Governorate
            dto.VideoUrl,
            dto.TextContent,
            dto.TargetKeywords,
            dto.AdType,
            dto.TargetGovernorates,
            dto.TargetCities,
            dto.TargetServices,
            dto.TargetUserGender,
            dto.TargetDays,
            dto.TargetMonths,
            dto.TargetTimeStart,
            dto.TargetTimeEnd
        );

        ad.SetMainImage(dto.ImageUrl);
        if (dto.IsActive) ad.Approve(); else ad.Reject();
        ad.SetDisplayOrder(dto.DisplayOrder);
        
        _context.Ads.Add(ad);
        await _context.SaveChangesAsync();
        
        return Ok(ApiResponse<int>.Succeed(ad.Id));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateAd(int id, [FromBody] EnhancedAdDto dto)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null || ad.IsDeleted) return NotFound();

        ad.UpdateDetails(
            dto.Title,
            dto.Description ?? "",
            dto.StartDate ?? DateTime.UtcNow,
            dto.EndDate ?? DateTime.UtcNow.AddMonths(1),
            dto.TargetUrl,
            dto.Placement,
            null,
            null,
            dto.VideoUrl,
            dto.TextContent,
            dto.TargetKeywords,
            dto.AdType,
            dto.TargetGovernorates,
            dto.TargetCities,
            dto.TargetServices,
            dto.TargetUserGender,
            dto.TargetDays,
            dto.TargetMonths,
            dto.TargetTimeStart,
            dto.TargetTimeEnd
        );

        ad.SetMainImage(dto.ImageUrl);
        ad.SetDisplayOrder(dto.DisplayOrder);

        if (dto.IsActive)
            ad.Approve();
        else
            ad.Reject();

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteAd(int id)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null) return NotFound();

        ad.IsDeleted = true;
        ad.DeletedAt = DateTime.UtcNow;
        // _context.Ads.Remove(ad); // Use soft delete
        await _context.SaveChangesAsync();
        
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
