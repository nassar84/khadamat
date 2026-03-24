using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/adpackages")]
public class AdPackagesController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public AdPackagesController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPackages()
    {
        var packages = await _context.AdPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => new AdPackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DurationDays = p.DurationDays,
                Tier = p.Tier,
                MaxAds = p.MaxAds,
                IsFeatured = p.IsFeatured,
                IsSponsored = p.IsSponsored,
                IsBanner = p.IsBanner,
                PriorityBoost = p.PriorityBoost,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AdPackageDto>>.Succeed(packages));
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAdPackageRequest request)
    {
        var package = new AdPackage(
            request.Name,
            request.Price,
            request.DurationDays,
            request.Tier,
            request.MaxAds,
            request.IsFeatured,
            request.IsSponsored,
            request.IsBanner,
            request.PriorityBoost,
            request.Description
        );

        _context.AdPackages.Add(package);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(package.Id));
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateAdPackageRequest request)
    {
        var package = await _context.AdPackages.FindAsync(id);
        if (package == null) return NotFound();

        package.UpdateDetails(
            request.Name,
            request.Price,
            request.DurationDays,
            request.Tier,
            request.MaxAds,
            request.IsFeatured,
            request.IsSponsored,
            request.IsBanner,
            request.PriorityBoost,
            request.Description
        );

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _context.AdPackages.FindAsync(id);
        if (package == null) return NotFound();

        package.Deactivate();
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
