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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using System.Net.Http.Json;
using System.IO;
using Khadamat.Infrastructure.Services;

namespace Khadamat.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse<AuthResponse>> ExternalTokenLoginAsync(string provider, string token)
    {
        string email, name, providerUserId, imageUrl = null;

        if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                });

                email = payload.Email;
                name = payload.Name;
                providerUserId = payload.Subject;
                imageUrl = payload.Picture;
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail("فشل التحقق من توكن جوجل: " + ex.Message);
            }
        }
        else if (provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var fbResponse = await client.GetFromJsonAsync<FacebookUserData>($"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={token}");
                
                if (fbResponse == null || string.IsNullOrEmpty(fbResponse.id))
                    return ApiResponse<AuthResponse>.Fail("فشل التحقق من توكن فيسبوك");

                email = fbResponse.email ?? $"{fbResponse.id}@facebook.com"; // Fallback if email not shared
                name = fbResponse.name;
                providerUserId = fbResponse.id;
                imageUrl = fbResponse.picture?.data?.url;
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail("فشل الاتصال بفيسبوك: " + ex.Message);
            }
        }
        else
        {
            return ApiResponse<AuthResponse>.Fail("مزود خدمة غير مدعوم");
        }

        return await ExternalLoginCallbackAsync(email, name, provider, providerUserId, imageUrl);
    }

    private class FacebookUserData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public FacebookPicture picture { get; set; }
    }

    private class FacebookPicture
    {
        public FacebookPictureData data { get; set; }
    }

    private class FacebookPictureData
    {
        public string url { get; set; }
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUserByEmail = await _userManager.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (existingUserByEmail)
        {
            return ApiResponse<AuthResponse>.Fail("البريد الإلكتروني مسجل مسبقاً.");
        }

        var existingUserByName = await _userManager.Users.AnyAsync(u => u.UserName.ToLower() == request.UserName.ToLower());
        if (existingUserByName)
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
            Gender = request.Gender,
            Role = role,
            IsProvider = false, // All start as regular users
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        if (string.IsNullOrEmpty(user.ProfileImageUrl) && string.IsNullOrEmpty(request.ProfileImageBase64))
        {
            // Default based on gender
            if (request.Gender == "Female")
                user.ProfileImageUrl = "https://cdn-icons-png.flaticon.com/512/6997/6997662.png"; // Placeholder female
            else
                user.ProfileImageUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png"; // Placeholder male
        }

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponse>.Fail("فشل إنشاء الحساب", errors);
        }

        // Save profile image as u_{userid}.jpg after successful user creation
        if (!string.IsNullOrEmpty(request.ProfileImageBase64))
        {
            user.ProfileImageUrl = await SaveUserProfileImageAsync(request.ProfileImageBase64, user.Id);
            await _userManager.UpdateAsync(user);
        }

        await _userManager.AddToRoleAsync(user, role.ToString());

        return await GenerateAuthResponse(user, "تم إنشاء الحساب بنجاح");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Try finding by username OR email (case-insensitive)
        var user = await _userManager.Users.FirstOrDefaultAsync(u => 
            u.UserName.ToLower() == request.UserName.ToLower() || 
            u.Email.ToLower() == request.UserName.ToLower());

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

    private void DeleteOldProfileImage(string? currentImageName)
    {
        if (string.IsNullOrEmpty(currentImageName) || currentImageName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users", currentImageName);
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting old profile image: {ex.Message}");
        }
    }

    private async Task<string?> SaveUserProfileImageAsync(string? base64OrUrlOrFilename, string userId)
    {
        if (string.IsNullOrEmpty(base64OrUrlOrFilename)) return null;

        // If it's a social external URL, keep it
        if (base64OrUrlOrFilename.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return base64OrUrlOrFilename;
        }

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var targetFileName = $"u_{userId}.jpg";
        var filePath = Path.Combine(folderPath, targetFileName);

        if (base64OrUrlOrFilename.StartsWith("data:", StringComparison.OrdinalIgnoreCase) || base64OrUrlOrFilename.Contains(","))
        {
            try
            {
                var data = base64OrUrlOrFilename.Contains(",") ? base64OrUrlOrFilename.Split(',')[1] : base64OrUrlOrFilename;
                var bytes = Convert.FromBase64String(data);
                await File.WriteAllBytesAsync(filePath, bytes);
                return targetFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving base64 profile image: {ex.Message}");
                return null;
            }
        }

        var cleanFilename = ImageNamingHelper.ExtractFileName(base64OrUrlOrFilename);
        if (string.IsNullOrEmpty(cleanFilename)) return null;

        return ImageNamingHelper.RenameImage(cleanFilename, "users", $"u_{userId}");
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
        user.Bio = request.Bio;
        user.WebsiteUrl = request.WebsiteUrl;
        user.InstagramUrl = request.InstagramUrl;
        user.TwitterUrl = request.TwitterUrl;
        user.FacebookUrl = request.FacebookUrl;
        user.LinkedInUrl = request.LinkedInUrl;
        user.TikTokUrl = request.TikTokUrl;
        user.Gender = request.Gender;

        // Process profile image
        var cleanRequestImage = ImageNamingHelper.ExtractFileName(request.ProfileImageUrl);
        if (cleanRequestImage != user.ProfileImageUrl)
        {
            DeleteOldProfileImage(user.ProfileImageUrl);
            user.ProfileImageUrl = await SaveUserProfileImageAsync(request.ProfileImageUrl, user.Id);
        }
        else
        {
            user.ProfileImageUrl = cleanRequestImage;
        }

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
            ImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            WebsiteUrl = user.WebsiteUrl,
            InstagramUrl = user.InstagramUrl,
            TwitterUrl = user.TwitterUrl,
            FacebookUrl = user.FacebookUrl,
            LinkedInUrl = user.LinkedInUrl,
            TikTokUrl = user.TikTokUrl,
            Gender = user.Gender
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

    public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangeMyPasswordRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ApiResponse<bool>.Fail("غير مصرح");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("المستخدم غير موجود");

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.Fail("فشل تغيير كلمة المرور", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<bool>.Succeed(true, "تم تغيير كلمة المرور بنجاح");
    }

    public async Task<ApiResponse<AuthResponse>> ExternalLoginCallbackAsync(string email, string name, string provider, string providerUserId, string? imageUrl = null)
    {
        var info = new UserLoginInfo(provider, providerUserId, provider);
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var baseUsername = email.Split('@')[0].Replace(".", "_");
                var username = baseUsername;
                var counter = 1;
                while (await _userManager.FindByNameAsync(username) != null)
                {
                    username = $"{baseUsername}{counter++}";
                }

                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = name,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = UserRole.Client,
                    EmailConfirmed = true,
                    ProfileImageUrl = imageUrl,
                    IsVerified = true // External providers usually verify email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return ApiResponse<AuthResponse>.Fail("فشل إنشاء مستخدم من خلال تسجيل الدخول الاجتماعي", createResult.Errors.Select(e => e.Description).ToList());
                }
                
                await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
            }
            else
            {
                // Update existing user image if missing
                if (string.IsNullOrEmpty(user.ProfileImageUrl) && !string.IsNullOrEmpty(imageUrl))
                {
                    user.ProfileImageUrl = imageUrl;
                    await _userManager.UpdateAsync(user);
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return ApiResponse<AuthResponse>.Fail("فشل ربط الحساب الاجتماعي");
            }
        }

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("الحساب معطل حالياً.");

        return await GenerateAuthResponse(user, "تم تسجيل الدخول بنجاح");
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Security: Don't reveal that the user doesn't exist
            return ApiResponse<bool>.Succeed(true, "إذا كان البريد الإلكتروني مسجلاً، فقد تم إرسال رابط إعادة تعيين كلمة المرور.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Generate reset link
        var webAppBaseUrl = _configuration["ApiSettings:WebAppBaseUrl"] ?? "http://localhost:5028/";
        var resetLink = $"{webAppBaseUrl}reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        // TODO: Send Email. For now, we will log it and return it in the response for development/testing
        Console.WriteLine($"Reset Password Link for {user.Email}: {resetLink}");

        return ApiResponse<bool>.Succeed(true, $"تم إرسال رابط إعادة التعيين بنجاح. (للتطوير: {resetLink})");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("المستخدم غير موجود.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            return ApiResponse<bool>.Succeed(true, "تم إعادة تعيين كلمة المرور بنجاح.");
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return ApiResponse<bool>.Fail("فشل إعادة تعيين كلمة المرور.", errors);
    }
}
