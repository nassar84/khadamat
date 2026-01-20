using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingUserByEmail != null)
        {
            return ApiResponse<AuthResponse>.Fail("البريد الإلكتروني مسجل مسبقاً.");
        }

        var existingUserByName = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserByName != null)
        {
            return ApiResponse<AuthResponse>.Fail("اسم المستخدم مسجل مسبقاً.");
        }

        if (!Enum.TryParse<UserRole>(request.UserType, true, out var role))
        {
            return ApiResponse<AuthResponse>.Fail("نوع المستخدم غير صالح.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            CityId = request.CityId,
            Role = role,
            IsProvider = false, // All start as regular users, can become providers later by creating a profile
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponse>.Fail("فشل إنشاء الحساب", errors);
        }

        await _userManager.AddToRoleAsync(user, role.ToString());

        return await GenerateAuthResponse(user, "تم إنشاء الحساب بنجاح");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
            return ApiResponse<AuthResponse>.Fail("بيانات الاعتماد غير صالحة.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return ApiResponse<AuthResponse>.Fail("بيانات الاعتماد غير صالحة.");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("الحساب معطل حالياً.");

        return await GenerateAuthResponse(user, "تم تسجيل الدخول بنجاح");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null) return ApiResponse<AuthResponse>.Fail("توكن غير صالح.");

        var email = principal.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.Fail("ريفريش توكن غير صالح أو منتهي الصلاحية.");
        }

        return await GenerateAuthResponse(user, "تم تجديد التوكن بنجاح");
    }

    public async Task<ApiResponse<AuthResponse>> GetProfileAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ApiResponse<AuthResponse>.Fail("غير مصرح");

        var user = await _userManager.Users
            .Include(u => u.City)
            .ThenInclude(c => c.Governorate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ApiResponse<AuthResponse>.Fail("المستخدم غير موجود");

        return await GenerateAuthResponse(user, "تم استرداد البيانات بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ApiResponse<bool>.Fail("غير مصرح");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("المستخدم غير موجود");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.CityId = request.CityId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.Fail("فشل تحديث البيانات", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<bool>.Succeed(true, "تم تحديث البيانات بنجاح");
    }

    private async Task<ApiResponse<AuthResponse>> GenerateAuthResponse(ApplicationUser user, string message)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var expiryMinutes = double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");

        return ApiResponse<AuthResponse>.Succeed(new AuthResponse
        {
            Id = user.Id,
            UserName = user.UserName!, // Return actual Username
            Email = user.Email!,
            Roles = roles.ToList(),
            Token = token,
            RefreshToken = refreshToken,
            IsVerified = user.IsVerified,
            IsProvider = user.IsProvider,
            Expiration = DateTime.UtcNow.AddMinutes(expiryMinutes),
            CityId = user.CityId,
            PhoneNumber = user.PhoneNumber,
            GovernorateId = user.City?.GovernorateId,
            CityName = user.City?.City_Name_AR,
            GovernorateName = user.City?.Governorate?.Governorate_Name_AR,
            FullName = user.FullName ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive,
            ImageUrl = user.ProfileImageUrl
        }, message);
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!), // Use UserName for the Name claim
            new Claim("is_provider", user.IsProvider.ToString().ToLower()),
            new Claim("is_verified", user.IsVerified.ToString().ToLower())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }

    public async Task<bool> SetUserIsProviderAsync(string userId, bool isProvider)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        
        user.IsProvider = isProvider;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
