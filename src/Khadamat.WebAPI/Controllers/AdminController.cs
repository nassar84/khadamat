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
[Authorize(Policy = "RequireAdmin")]
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
        var currentUser = await _userManager.GetUserAsync(User);
        var isSuperAdmin = currentUser != null && await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");

        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            // If requester is not SuperAdmin, hide SuperAdmin users
            if (!isSuperAdmin && role == "SuperAdmin") continue;

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Role = role,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt
            });
        }

        return Ok(ApiResponse<List<UserDto>>.Succeed(userDtos));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(ApiResponse<bool>.Fail("البريد الإلكتروني وكلمة المرور مطلوبان"));
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(ApiResponse<bool>.Fail("البريد الإلكتروني مستخدم بالفعل"));
        }

        // Only SuperAdmin can create SuperAdmin users
        if (dto.Role == "SuperAdmin" && !User.IsInRole("SuperAdmin"))
        {
            return Forbid("Only SuperAdmin can create SuperAdmin users.");
        }

        // Create new user
        var newUser = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            CityId = dto.CityId,
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(newUser, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        // Assign role
        var roleToAssign = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
        await _userManager.AddToRoleAsync(newUser, roleToAssign);

        return Ok(ApiResponse<UserDto>.Succeed(new UserDto
        {
            Id = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email ?? "",
            PhoneNumber = newUser.PhoneNumber,
            Role = roleToAssign,
            IsActive = newUser.IsActive,
            IsVerified = newUser.IsVerified,
            ProfileImageUrl = newUser.ProfileImageUrl,
            CreatedAt = newUser.CreatedAt
        }));
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email; // Standard identity behavior
        user.PhoneNumber = dto.PhoneNumber;
        user.IsActive = dto.IsActive;
        user.IsVerified = dto.IsVerified;
        user.ProfileImageUrl = dto.ProfileImageUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Check permissions: Only SuperAdmin can promote/demote to SuperAdmin
        if (role == "SuperAdmin" && !User.IsInRole("SuperAdmin"))
        {
            return Forbid("Only SuperAdmin can manage SuperAdmin roles.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove old roles
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        
        // Add new role
        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("users/change-password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(ApiResponse<bool>.Fail("بيانات غير مكتملة"));
        }

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound();

        // Check permissions: Only SuperAdmin can change SuperAdmin passwords
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains("SuperAdmin") && !User.IsInRole("SuperAdmin"))
        {
            return Forbid("Only SuperAdmin can change SuperAdmin passwords.");
        }

        // Use Remove/Add password to force update without needing current password
        var removeResult = await _userManager.RemovePasswordAsync(user);
        // If user didn't have a password (e.g. external login), it's fine.
        
        var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!addResult.Succeeded)
        {
            return BadRequest(ApiResponse<bool>.Fail(string.Join(", ", addResult.Errors.Select(e => e.Description))));
        }

        return Ok(ApiResponse<bool>.Succeed(true));
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

        _context.ProviderProfiles.Remove(provider);
        await _context.SaveChangesAsync();
        
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("users/{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return Ok(ApiResponse<bool>.Succeed(user.IsActive));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);
        
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("services/{id}/approve")]
    public async Task<IActionResult> ApproveService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        service.Approve();
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("services/{id}/reject")]
    public async Task<IActionResult> RejectService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        // Use domain method for rejection before removal if needed, 
        // or just remove. Sticking to removal as per previous implementation.
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpGet("audit-logs/recent")]
    public async Task<IActionResult> GetRecentAuditLogs()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(10)
            .Select(l => new
            {
                Id = l.Id,
                Title = l.Action, // Using Action as title
                Time = l.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Icon = "fa-solid fa-list-ul" // Generic icon
            })
            .ToListAsync();

        return Ok(ApiResponse<List<object>>.Succeed(logs.Cast<object>().ToList()));
    }
}
