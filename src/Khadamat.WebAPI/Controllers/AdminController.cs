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

        Console.WriteLine($"[GetAllUsers] Current User: {currentUser?.Email}, IsSuperAdmin: {isSuperAdmin}");

        var users = await _userManager.Users.ToListAsync();
        Console.WriteLine($"[GetAllUsers] Total users in DB: {users.Count}");

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            Console.WriteLine($"[GetAllUsers] User: {user.Email}, Role: {role}");

            // Restriction: SystemAdmin can only see/manage regular users and providers
            // SuperAdmin can see everyone
            // BUT: Always allow current user to see their own account
            if (!isSuperAdmin && (role == "SuperAdmin" || role == "SystemAdmin") && user.Id != currentUser?.Id)
            {
                Console.WriteLine($"[GetAllUsers] Skipping {user.Email} - Admin account (not self)");
                continue; // Skip admin accounts if current user is not SuperAdmin, unless it's themselves
            }

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                CityId = user.CityId,
                Role = role,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                ProfileImageUrl = user.ProfileImageUrl,
                Gender = user.Gender,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                WebsiteUrl = user.WebsiteUrl,
                InstagramUrl = user.InstagramUrl,
                TwitterUrl = user.TwitterUrl,
                FacebookUrl = user.FacebookUrl,
                LinkedInUrl = user.LinkedInUrl,
                TikTokUrl = user.TikTokUrl
            });
        }

        Console.WriteLine($"[GetAllUsers] Returning {userDtos.Count} users");
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

        if (!User.IsInRole("SuperAdmin") && (dto.Role == "SuperAdmin" || dto.Role == "SystemAdmin"))
        {
            return Forbid("لا يمكنك إنشاء مستخدمين بصلاحيات إدارية.");
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
        if (user == null) return NotFound(ApiResponse<bool>.Fail("المستخدم غير موجود"));

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole("SuperAdmin") && (targetRoles.Contains("SuperAdmin") || targetRoles.Contains("SystemAdmin")))
        {
            return Forbid("لا يمكنك تعديل بيانات هذا المستخدم.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.UserName;
        user.PhoneNumber = dto.PhoneNumber;
        user.CityId = dto.CityId;
        user.IsActive = dto.IsActive;
        user.IsVerified = dto.IsVerified;
        user.ProfileImageUrl = dto.ProfileImageUrl;
        user.Gender = dto.Gender;
        user.Bio = dto.Bio;
        user.WebsiteUrl = dto.WebsiteUrl;
        user.InstagramUrl = dto.InstagramUrl;
        user.TwitterUrl = dto.TwitterUrl;
        user.FacebookUrl = dto.FacebookUrl;
        user.LinkedInUrl = dto.LinkedInUrl;
        user.TikTokUrl = dto.TikTokUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) 
            return BadRequest(ApiResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));

        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPost("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole("SuperAdmin") && (targetRoles.Contains("SuperAdmin") || targetRoles.Contains("SystemAdmin")))
        {
            return Forbid("لا يمكنك تعديل أدوار هذا المستخدم.");
        }

        if (role == "SuperAdmin" || role == "SystemAdmin")
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid("لا يمكنك تعيين أدوار إدارية.");
            }
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

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole("SuperAdmin") && (targetRoles.Contains("SuperAdmin") || targetRoles.Contains("SystemAdmin")))
        {
            return Forbid("لا يمكنك تغيير كلمة مرور هذا المستخدم.");
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


        var targetRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole("SuperAdmin") && (targetRoles.Contains("SuperAdmin") || targetRoles.Contains("SystemAdmin")))
        {
            return Forbid("لا يمكنك تغيير حالة هذا المستخدم.");
        }

        // Prevent toggling own status
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (id == currentUserId)
        {
            return BadRequest(ApiResponse<bool>.Fail("لا يمكنك تعطيل حسابك الشخصي."));
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return Ok(ApiResponse<bool>.Succeed(user.IsActive));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound(ApiResponse<bool>.Fail("المستخدم غير موجود"));

        var targetRoles = await _userManager.GetRolesAsync(user);
        if (!User.IsInRole("SuperAdmin") && (targetRoles.Contains("SuperAdmin") || targetRoles.Contains("SystemAdmin")))
        {
            return Forbid("لا يمكنك حذف هذا المستخدم.");
        }

        // Prevent deleting own account
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (id == currentUserId)
        {
            return BadRequest(ApiResponse<bool>.Fail("لا يمكنك حذف حسابك الشخصي."));
        }

        // Soft Delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = currentUserId;
        user.IsActive = false; // Also de-activate

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) 
            return BadRequest(ApiResponse<bool>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));
        
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

        // Soft Delete
        service.Reject("تم الرفض أو الحذف بواسطة المدير");
        // We set IsDeleted for soft delete as configured in DbContext
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        // Use reflection or just access property if base type is known. 
        // Service inherits from BaseEntity which has IsDeleted.
        service.IsDeleted = true;
        service.DeletedAt = DateTime.UtcNow;
        service.DeletedBy = currentUserId;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Succeed(true));
    }

    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDto dto)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound(ApiResponse<bool>.Fail("الخدمة غير موجودة"));

        service.UpdateDetails(
            dto.Title, 
            dto.Description, 
            dto.Address, 
            dto.Price,
            dto.Phone1,
            dto.Phone2,
            dto.WhatsApp,
            dto.Facebook,
            dto.Telegram,
            dto.WorkDays,
            dto.WorkHours
        );

        if (dto.CategoryId.HasValue || dto.SubCategoryId.HasValue)
        {
            service.SetCategory(dto.CategoryId, dto.SubCategoryId);
        }

        if (dto.CityId.HasValue)
        {
            service.UpdateLocation(dto.CityId);
        }

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
