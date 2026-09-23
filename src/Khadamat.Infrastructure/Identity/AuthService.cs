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
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _emailService = emailService;
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
                return ApiResponse<AuthResponse>.Fail("›‘· «· Õﬁﬁ „‰  Êﬂ‰ ÃÊÃ·: " + ex.Message);
            }
        }
        else if (provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var fbResponse = await client.GetFromJsonAsync<FacebookUserData>($"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={token}");
                
                if (fbResponse == null || string.IsNullOrEmpty(fbResponse.id))
                    return ApiResponse<AuthResponse>.Fail("›‘· «· Õﬁﬁ „‰  Êﬂ‰ ›Ì”»Êﬂ");

                email = fbResponse.email ?? $"{fbResponse.id}@facebook.com"; // Fallback if email not shared
                name = fbResponse.name;
                providerUserId = fbResponse.id;
                imageUrl = fbResponse.picture?.data?.url;
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail("›‘· «·« ’«· »›Ì”»Êﬂ: " + ex.Message);
            }
        }
        else
        {
            return ApiResponse<AuthResponse>.Fail("„ﬁœ„ Œœ„… €Ì— „œ⁄Ê„");
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
            return ApiResponse<AuthResponse>.Fail("«·»—Ìœ «·≈·ﬂ —Ê‰Ì „”Ã· „”»ﬁ«.");
        }

        var existingUserByName = await _userManager.Users.AnyAsync(u => u.UserName.ToLower() == request.UserName.ToLower());
        if (existingUserByName)
        {
            return ApiResponse<AuthResponse>.Fail("«”„ «·„” Œœ„ „”Ã· „”»ﬁ«.");
        }

        if (!Enum.TryParse<UserRole>(request.UserType, true, out var role))
        {
            return ApiResponse<AuthResponse>.Fail("‰Ê⁄ «·„” Œœ„ €Ì— ’«·Õ.");
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
            return ApiResponse<AuthResponse>.Fail("›‘· ≈‰‘«¡ «·Õ”«»", errors);
        }

        // Save profile image as u_{userid}.jpg after successful user creation
        if (!string.IsNullOrEmpty(request.ProfileImageBase64))
        {
            user.ProfileImageUrl = await SaveUserProfileImageAsync(request.ProfileImageBase64, user.Id);
            await _userManager.UpdateAsync(user);
        }

        // Save profile image as u_{userid}.jpg after successful user creation
        if (!string.IsNullOrEmpty(request.ProfileImageBase64))
        {
            user.ProfileImageUrl = await SaveUserProfileImageAsync(request.ProfileImageBase64, user.Id);
            await _userManager.UpdateAsync(user);
        }

        await _userManager.AddToRoleAsync(user, role.ToString());

        return await GenerateAuthResponse(user, " „ ≈‰‘«¡ «·Õ”«» »‰Ã«Õ");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // Try finding by username OR email (case-insensitive)
        var user = await _userManager.Users.FirstOrDefaultAsync(u => 
            u.UserName.ToLower() == request.UserName.ToLower() || 
            u.Email.ToLower() == request.UserName.ToLower());

        if (user == null)
            return ApiResponse<AuthResponse>.Fail("»Ì«‰«  «·«⁄ „«œ €Ì— ’«·Õ….");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return ApiResponse<AuthResponse>.Fail("»Ì«‰«  «·«⁄ „«œ €Ì— ’«·Õ….");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("«·Õ”«» „⁄ÿ· Õ«·Ì«.");

        return await GenerateAuthResponse(user, " „  ”ÃÌ· «·œŒÊ· »‰Ã«Õ");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null) return ApiResponse<AuthResponse>.Fail(" Êﬂ‰ €Ì— ’«·Õ.");

        var email = principal.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email!);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<AuthResponse>.Fail("—Ì›—Ì‘  Êﬂ‰ €Ì— ’«·Õ √Ê „‰ ÂÌ «·’·«ÕÌ….");
        }

        return await GenerateAuthResponse(user, " „  ÃœÌœ «· Êﬂ‰ »‰Ã«Õ");
    }

    public async Task<ApiResponse<AuthResponse>> GetProfileAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ApiResponse<AuthResponse>.Fail("€Ì— „’—Õ");

        var user = await _userManager.Users
            .Include(u => u.City)
            .ThenInclude(c => c.Governorate)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return ApiResponse<AuthResponse>.Fail("«·„” Œœ„ €Ì— „ÊÃÊœ");

        return await GenerateAuthResponse(user, " „ «” —œ«œ «·»Ì«‰«  »‰Ã«Õ");
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
        if (string.IsNullOrEmpty(userId)) return ApiResponse<bool>.Fail("€Ì— „’—Õ");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("«·„” Œœ„ €Ì— „ÊÃÊœ");

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
            return ApiResponse<bool>.Fail("›‘·  ÕœÌÀ «·»Ì«‰« ", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<bool>.Succeed(true, " „  ÕœÌÀ «·»Ì«‰«  »‰Ã«Õ");
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
            ImageUrl = !string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !user.ProfileImageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && !user.ProfileImageUrl.StartsWith("images/", StringComparison.OrdinalIgnoreCase) 
                ? $"images/users/{user.ProfileImageUrl}" 
                : user.ProfileImageUrl,
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
        if (string.IsNullOrEmpty(userId)) return ApiResponse<bool>.Fail("€Ì— „’—Õ");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("«·„” Œœ„ €Ì— „ÊÃÊœ");

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            return ApiResponse<bool>.Fail("›‘·  €ÌÌ— ﬂ·„… «·„—Ê—", result.Errors.Select(e => e.Description).ToList());
        }

        return ApiResponse<bool>.Succeed(true, " „  €ÌÌ— ﬂ·„… «·„—Ê— »‰Ã«Õ");
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
                // Use FullName as base for UserName (remove spaces & special chars for Identity compliance)
                var rawBase = !string.IsNullOrWhiteSpace(name)
                    ? name.Trim()
                    : email.Split('@')[0];
                var baseUsername = System.Text.RegularExpressions.Regex.Replace(rawBase, @"[^a-zA-Z0-9\u0600-\u06FF_]", "_").Trim('_');
                if (string.IsNullOrEmpty(baseUsername))
                    baseUsername = email.Split('@')[0].Replace(".", "_");
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
                    return ApiResponse<AuthResponse>.Fail("›‘· ≈‰‘«¡ „” Œœ„ „‰ Œ·«·  ”ÃÌ· «·œŒÊ· «·«Ã „«⁄Ì", createResult.Errors.Select(e => e.Description).ToList());
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
                return ApiResponse<AuthResponse>.Fail("›‘· —»ÿ «·Õ”«» «·«Ã „«⁄Ì");
            }
        }
        else
        {
            // Existing user logged in again
            bool updated = false;
            if (!string.IsNullOrEmpty(imageUrl) && 
                (string.IsNullOrEmpty(user.ProfileImageUrl) || 
                 (!user.ProfileImageUrl.Contains("access_token") && user.ProfileImageUrl.Contains("graph.facebook.com")) ||
                 user.ProfileImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
            {
                user.ProfileImageUrl = imageUrl;
                updated = true;
            }

            if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(name))
            {
                user.FullName = name;
                updated = true;
            }

            if (updated)
            {
                await _userManager.UpdateAsync(user);
            }
        }

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("«·Õ”«» „⁄ÿ· Õ«·Ì«.");

        return await GenerateAuthResponse(user, " „  ”ÃÌ· «·œŒÊ· »‰Ã«Õ");
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Security: Don't reveal that the user doesn't exist
            return ApiResponse<bool>.Succeed(true, "≈–« ﬂ«‰ «·»—Ìœ «·≈·ﬂ —Ê‰Ì „”Ã·« ·œÌ‰«° ›”  ·ﬁÏ —«»ÿ« ·≈⁄«œ…  ⁄ÌÌ‰ ﬂ·„… «·„—Ê— ⁄»— »—Ìœﬂ «·≈·ﬂ —Ê‰Ì.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Generate reset link using configured WebAppBaseUrl, current host, or production URL
        var webAppBaseUrl = _configuration["ApiSettings:WebAppBaseUrl"];
        if (string.IsNullOrWhiteSpace(webAppBaseUrl))
        {
            var req = _httpContextAccessor.HttpContext?.Request;
            if (req != null && !string.IsNullOrEmpty(req.Host.Value))
            {
                webAppBaseUrl = $"{req.Scheme}://{req.Host.Value}/";
            }
            else
            {
                webAppBaseUrl = "https://khadamawy.eis-dev.com/";
            }
        }
        if (!webAppBaseUrl.EndsWith("/")) webAppBaseUrl += "/";

        var resetLink = $"{webAppBaseUrl}reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        Console.WriteLine($"[PasswordReset] Generated link for {user.Email}: {resetLink}");

        // Send Email to user
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName ?? user.UserName ?? "⁄“Ì“‰« «·⁄„Ì·", resetLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PasswordReset] Failed to send email to {user.Email}: {ex.Message}");
        }

        return ApiResponse<bool>.Succeed(true, " „ ≈—”«· —«»ÿ ≈⁄«œ…  ⁄ÌÌ‰ ﬂ·„… «·„—Ê— ≈·Ï »—Ìœﬂ «·≈·ﬂ —Ê‰Ì »‰Ã«Õ. Ì—ÃÏ „—«Ã⁄… ’‰œÊﬁ «·Ê«—œ «·Œ«’ »ﬂ.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("«·„” Œœ„ €Ì— „ÊÃÊœ.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            return ApiResponse<bool>.Succeed(true, " „ ≈⁄«œ…  ⁄ÌÌ‰ ﬂ·„… «·„—Ê— »‰Ã«Õ.");
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return ApiResponse<bool>.Fail("›‘· ≈⁄«œ…  ⁄ÌÌ‰ ﬂ·„… «·„—Ê—.", errors);
    }

    public async Task<ApiResponse<bool>> DeleteAccountAsync(string? password = null)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return ApiResponse<bool>.Fail("€Ì— „’—Õ");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("«·„” Œœ„ €Ì— „ÊÃÊœ");

        // If user has a password and password was provided, verify it
        if (!string.IsNullOrEmpty(user.PasswordHash) && !string.IsNullOrEmpty(password))
        {
            var validPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!validPassword)
            {
                return ApiResponse<bool>.Fail("ﬂ·„… «·„—Ê— €Ì— ’ÕÌÕ…");
            }
        }

        // Anonymize & Deactivate user (Google Play account deletion compliance)
        user.IsActive = false;
        user.Email = $"deleted_{user.Id}@khadamawy.deleted";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = $"deleted_{user.Id}";
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.FullName = "Õ”«» „Õ–Ê›";
        user.PhoneNumber = null;
        user.ProfileImageUrl = null;
        user.Bio = null;
        user.WebsiteUrl = null;
        user.InstagramUrl = null;
        user.TwitterUrl = null;
        user.FacebookUrl = null;
        user.LinkedInUrl = null;
        user.TikTokUrl = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ApiResponse<bool>.Fail("›‘· ›Ì „⁄«·Ã… Õ–› «·Õ”«»");
        }

        await _signInManager.SignOutAsync();
        return ApiResponse<bool>.Succeed(true, " „ Õ–› Ê ⁄ÿÌ· «·Õ”«» »‰Ã«Õ");
    }
}
