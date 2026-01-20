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
public class ProvidersController : ControllerBase
{
    private readonly KhadamatDbContext _context;

    public ProvidersController(KhadamatDbContext context)
    {
        _context = context;
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> Apply([FromBody] ApplyProviderDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var existingProfile = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existingProfile != null)
        {
            return BadRequest("You have already applied or are already a provider.");
        }

        var profile = new ProviderProfile
        {
            UserId = userId,
            BusinessName = dto.BusinessName,
            Bio = dto.Bio,
            CityId = dto.CityId,
            ContactNumber = dto.ContactNumber,
            WebsiteUrl = dto.WebsiteUrl,
            IdCardImage = dto.IdCardImage,
            CertificateImage = dto.CertificateImage,
            Verified = false,
            CreatedAt = DateTime.UtcNow,
            Photo = "" // Default or handle upload
        };

        _context.ProviderProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<int>.Succeed(profile.Id));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetProfile(string userId)
    {
         var profile = await _context.ProviderProfiles
             .Include(p => p.City)
             .FirstOrDefaultAsync(p => p.UserId == userId);
             
         if (profile == null) return NotFound();
         
         return Ok(profile);
    }
}
