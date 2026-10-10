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
using Khadamat.Infrastructure.Services;
using System.IO;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/ads")]
public class AdsController : ControllerBase
{
    private readonly KhadamatDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly Khadamat.Application.Interfaces.IImageStorageService _imageStorage;

    public AdsController(KhadamatDbContext context, IWebHostEnvironment env, Khadamat.Application.Interfaces.IImageStorageService imageStorage)
    {
        _context = context;
        _env = env;
        _imageStorage = imageStorage;
    }

    // Public Endpoint: Get active ads for slider or specific placement
    [HttpGet("placements/{placement}")]
    public async Task<IActionResult> GetAdsByPlacement(string placement)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var ads = await _context.Ads
            .Where(a => !a.IsDeleted && a.Approved && a.Placement == placement && 
                        a.StartDate.Date <= today && a.EndDate.Date >= today)
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
                ServiceId = a.ServiceID,
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
        var ads = await (from a in _context.Ads.Where(a => !a.IsDeleted)
                         join u in _context.Users on a.UserCreated equals u.Id into userGroup
                         from u in userGroup.DefaultIfEmpty()
                         join s in _context.Services on a.ServiceID equals s.Id into serviceGroup
                         from s in serviceGroup.DefaultIfEmpty()
                         orderby a.CreatedAt descending
                         select new EnhancedAdDto
                         {
                             Id = a.Id,
                             Title = a.Title,
                             Description = a.Description,
                             AdType = a.AdType,
                             ImageUrl = a.ImagePath,
                             VideoUrl = a.VideoUrl,
                             TextContent = a.TextContent,
                             TargetUrl = a.RedirectUrl,
                             ServiceId = a.ServiceID,
                             TargetCategories = a.CategoryID.HasValue ? a.CategoryID.ToString() : null,
                             TargetSubCategories = a.SubCategoryID.HasValue ? a.SubCategoryID.ToString() : null,
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
                             TargetTimeEnd = a.TargetTimeEnd,
                             TargetDeepSubCategories = a.TargetDeepSubCategories,
                             AmountPaid = a.AmountPaid,
                             CreatedBy = a.UserCreated,
                             AdvertiserName = u != null ? u.FullName : (s != null ? s.Name : null)
                         }).ToListAsync();

        return Ok(ApiResponse<List<EnhancedAdDto>>.Succeed(ads));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetAdById(int id)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null || ad.IsDeleted) return NotFound();

        string? advertiserName = null;
        if (!string.IsNullOrEmpty(ad.UserCreated))
        {
            var user = await _context.Users.FindAsync(ad.UserCreated);
            advertiserName = user?.FullName;
        }

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
            ServiceId = ad.ServiceID,
            TargetCategories = ad.CategoryID.HasValue ? ad.CategoryID.ToString() : null,
            TargetSubCategories = ad.SubCategoryID.HasValue ? ad.SubCategoryID.ToString() : null,
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
            TargetTimeEnd = ad.TargetTimeEnd,
            TargetDeepSubCategories = ad.TargetDeepSubCategories,
            AmountPaid = ad.AmountPaid,
            CreatedBy = ad.UserCreated,
            AdvertiserName = advertiserName
        };

        return Ok(ApiResponse<EnhancedAdDto>.Succeed(dto));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> CreateAd([FromBody] EnhancedAdDto dto)
    {
        // Parse category IDs
        int? categoryId = null;
        if (int.TryParse(dto.TargetCategories, out int cid)) categoryId = cid;

        int? subCategoryId = null;
        if (int.TryParse(dto.TargetSubCategories, out int scid)) subCategoryId = scid;

        var ad = new Ad(
            dto.Title, 
            dto.Description ?? "", 
            dto.StartDate ?? DateTime.UtcNow, 
            dto.EndDate ?? DateTime.UtcNow.AddMonths(1),
            dto.AdType ?? "Image",
            null, // ActivityId
            categoryId,
            subCategoryId,
            dto.ServiceId // Linked service ID
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
            dto.TargetDeepSubCategories,
            dto.TargetUserGender,
            dto.TargetDays,
            dto.TargetMonths,
            dto.TargetTimeStart,
            dto.TargetTimeEnd,
            categoryId,
            subCategoryId,
            dto.ServiceId, // Linked service ID
            dto.AmountPaid
        );

        // Determine owner (advertiser)
        var currentAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        string? ownerId = null;
        if (!string.IsNullOrWhiteSpace(dto.CreatedBy))
        {
            ownerId = dto.CreatedBy;
        }
        else if (dto.ServiceId.HasValue)
        {
            var svc = await _context.Services.FindAsync(dto.ServiceId.Value);
            if (svc != null && !string.IsNullOrEmpty(svc.UserCreated))
            {
                ownerId = svc.UserCreated;
            }
        }
        
        if (string.IsNullOrEmpty(ownerId))
        {
            ownerId = currentAdminId;
        }

        ad.SetOwner(ownerId);

        if (dto.IsActive) ad.Approve(); else ad.Reject();
        ad.SetDisplayOrder(dto.DisplayOrder);
        
        _context.Ads.Add(ad);
        await _context.SaveChangesAsync(); // Save first to get ad.Id

        if (!string.IsNullOrEmpty(dto.ImageBase64))
        {
            var imageUrl = await SaveImage(dto.ImageBase64, ad.Id);
            ad.SetMainImage(imageUrl);
            await _context.SaveChangesAsync();
        }
        else if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            var cleanFilename = ImageNamingHelper.ExtractFileName(dto.ImageUrl);
            var finalName = ImageNamingHelper.RenameImage(cleanFilename, "ads", $"ad_{ad.Id}_1");
            ad.SetMainImage(finalName);
            await _context.SaveChangesAsync();
        }
        
        return Ok(ApiResponse<int>.Succeed(ad.Id));
    }

    private async Task<string> SaveImage(string base64Data, int adId)
    {
        try
        {
            var fileName = $"ad_{adId}_1.jpg";
            var filePath = _imageStorage.GetFilePath("ads", fileName);

            var data = base64Data.Contains(",") ? base64Data.Split(',')[1] : base64Data;
            var bytes = Convert.FromBase64String(data);
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            return fileName; // Return ONLY the filename!
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving ad image: {ex.Message}");
            return "default-ad.png"; // Return only the filename!
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateAd(int id, [FromBody] EnhancedAdDto dto)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null || ad.IsDeleted) return NotFound();

        int? categoryId = int.TryParse(dto.TargetCategories, out int cid2) ? cid2 : null;
        int? subCategoryId = int.TryParse(dto.TargetSubCategories, out int scid2) ? scid2 : null;

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
            dto.TargetDeepSubCategories,
            dto.TargetUserGender,
            dto.TargetDays,
            dto.TargetMonths,
            dto.TargetTimeStart,
            dto.TargetTimeEnd,
            categoryId,
            subCategoryId,
            dto.ServiceId, // Fixed: Pass dto.ServiceId
            dto.AmountPaid
        );

        if (!string.IsNullOrWhiteSpace(dto.CreatedBy))
        {
            ad.SetOwner(dto.CreatedBy);
        }
        else if (dto.ServiceId.HasValue && string.IsNullOrEmpty(ad.UserCreated))
        {
            var svc = await _context.Services.FindAsync(dto.ServiceId.Value);
            if (svc != null && !string.IsNullOrEmpty(svc.UserCreated))
            {
                ad.SetOwner(svc.UserCreated);
            }
        }

        if (!string.IsNullOrEmpty(dto.ImageBase64))
        {
            // Delete old file
            var currentFilename = ImageNamingHelper.ExtractFileName(ad.ImagePath);
            if (!string.IsNullOrEmpty(currentFilename))
            {
                _imageStorage.DeleteFile("ads", currentFilename);
            }

            var imageUrl = await SaveImage(dto.ImageBase64, ad.Id);
            ad.SetMainImage(imageUrl);
        }
        else if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            var cleanFilename = ImageNamingHelper.ExtractFileName(dto.ImageUrl);
            var currentFilename = ImageNamingHelper.ExtractFileName(ad.ImagePath);
            if (!string.IsNullOrEmpty(currentFilename) && currentFilename != cleanFilename)
            {
                _imageStorage.DeleteFile("ads", currentFilename);
            }

            var finalName = ImageNamingHelper.RenameImage(cleanFilename, "ads", $"ad_{ad.Id}_1");
            ad.SetMainImage(finalName);
        }
        else
        {
            // Clear image
            var currentFilename = ImageNamingHelper.ExtractFileName(ad.ImagePath);
            if (!string.IsNullOrEmpty(currentFilename))
            {
                _imageStorage.DeleteFile("ads", currentFilename);
            }
            ad.SetMainImage(null);
        }

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

    [HttpPost("{id}/track-view")]
    public async Task<IActionResult> TrackView(int id)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null) return NotFound();
        ad.IncrementViews();
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/track-click")]
    public async Task<IActionResult> TrackClick(int id)
    {
        var ad = await _context.Ads.FindAsync(id);
        if (ad == null) return NotFound();
        ad.IncrementClicks();
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyAds()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var ads = await _context.Ads
            .Where(a => !a.IsDeleted && a.UserCreated == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new EnhancedAdDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                AdType = a.AdType,
                ImageUrl = a.ImagePath,
                TargetUrl = a.RedirectUrl,
                Placement = a.Placement,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                IsActive = a.Approved,
                ViewCount = a.Views,
                ClickCount = a.Clicks,
                AmountPaid = a.AmountPaid,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<EnhancedAdDto>>.Succeed(ads));
    }
}
