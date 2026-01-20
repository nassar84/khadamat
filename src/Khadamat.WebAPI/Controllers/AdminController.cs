using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Infrastructure.Identity;
using Khadamat.Infrastructure.Persistence;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SystemAdmin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly KhadamatDbContext _context;

    public AdminController(UserManager<ApplicationUser> userManager, KhadamatDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "User",
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            });
        }

        return Ok(ApiResponse<List<UserDto>>.Succeed(userDtos));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats()
    {
        var stats = new AdminStatsDto
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalProviders = await _context.ProviderProfiles.CountAsync(),
            TotalServices = await _context.Services.CountAsync(),
            TotalOrders = 0 // Placeholder until orders are implemented
        };

        return Ok(ApiResponse<AdminStatsDto>.Succeed(stats));
    }

    [HttpGet("providers/pending")]
    public async Task<ActionResult<ApiResponse<List<PendingProviderDto>>>> GetPendingProviders()
    {
        var pendingProviders = await _context.ProviderProfiles
            .Where(p => !p.Verified)
            .Include(p => p.City)
            .ToListAsync();

        var result = new List<PendingProviderDto>();

        foreach (var p in pendingProviders)
        {
            var user = await _userManager.FindByIdAsync(p.UserId);
            if (user != null)
            {
                result.Add(new PendingProviderDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Name = user.FullName,
                    Email = user.Email ?? "",
                    BusinessName = p.BusinessName,
                    Phone = p.ContactNumber,
                    City = p.City?.City_Name_AR ?? "",
                    Expertise = p.Bio,
                    JoinedAt = p.CreatedAt,
                    IdCardImage = p.IdCardImage ?? "",
                    CertificateImage = p.CertificateImage ?? ""
                });
            }
        }

        return Ok(ApiResponse<List<PendingProviderDto>>.Succeed(result));
    }

    [HttpPost("providers/{id}/approve")]
    public async Task<IActionResult> ApproveProvider(int id)
    {
        var provider = await _context.ProviderProfiles.FindAsync(id);
        if (provider == null) return NotFound();

        provider.Verified = true;
        
        var user = await _userManager.FindByIdAsync(provider.UserId);
        if (user != null)
        {
            user.IsProvider = true;
            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("providers/{id}/reject")]
    public async Task<IActionResult> RejectProvider(int id)
    {
        var provider = await _context.ProviderProfiles.FindAsync(id);
        if (provider == null) return NotFound();

        // For rejection, we simply delete the profile so they can apply again, or keep it as unverified?
        // UI assumes rejection removes it from pending list.
        // If we keep it unverified, it stays in pending list (Verified=false).
        // So we should probably delete it or mark a status if we had one.
        // Given we only have Verified bool, I'll delete it.
        
        _context.ProviderProfiles.Remove(provider);
        await _context.SaveChangesAsync();
        
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
