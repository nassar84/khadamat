using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly Microsoft.AspNetCore.Identity.SignInManager<Khadamat.Infrastructure.Identity.ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService, 
        Microsoft.AspNetCore.Identity.SignInManager<Khadamat.Infrastructure.Identity.ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _authService = authService;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet("external-login")]
    public IActionResult ExternalLogin(string provider, string redirectUrl)
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, 
            Url.Action("ExternalLoginCallback", new { redirectUrl }));
        return Challenge(properties, provider);
    }

    [HttpPost("external-token-login")]
    public async Task<IActionResult> ExternalTokenLogin([FromBody] ExternalTokenLoginRequest request)
    {
        var result = await _authService.ExternalTokenLoginAsync(request.Provider, request.Token);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string redirectUrl, string? remoteError = null)
    {
        try
        {
            if (remoteError != null)
            {
                return Redirect($"{redirectUrl}?error={Uri.EscapeDataString(remoteError)}");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect($"{redirectUrl}?error=failed_to_get_external_login_info");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var provider = info.LoginProvider;
            var providerUserId = info.ProviderKey;

            // Extract Profile Image from mapped claims (both Google & Facebook map to "picture", Google also maps to "urn:google:image")
            var imageUrl = info.Principal.FindFirstValue("picture") 
                           ?? info.Principal.FindFirstValue("urn:google:image")
                           ?? info.Principal.FindFirstValue("urn:google:picture")
                           ?? info.Principal.FindFirstValue(ClaimTypes.Uri);

            // If picture wasn't in claims and provider is Facebook, construct authenticated Graph API picture URL with App Token
            if (string.IsNullOrEmpty(imageUrl) && provider == "Facebook")
            {
                var appId = _configuration["Authentication:Facebook:AppId"];
                var appSecret = _configuration["Authentication:Facebook:AppSecret"];
                if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appSecret))
                {
                    imageUrl = $"https://graph.facebook.com/v19.0/{providerUserId}/picture?type=large&access_token={appId}|{appSecret}";
                }
            }

            // Facebook may not return email if user's privacy settings restrict it.
            // Generate a fallback unique email using the provider user ID.
            if (string.IsNullOrEmpty(email))
            {
                email = $"{provider.ToLower()}_{providerUserId}@khadamat.app";
            }

            // Fallback for display name
            if (string.IsNullOrEmpty(name))
            {
                name = email.Split('@')[0];
            }

            var result = await _authService.ExternalLoginCallbackAsync(email, name, provider, providerUserId, imageUrl);

            if (result.Success && result.Data != null)
            {
                return Redirect($"{redirectUrl}?token={result.Data.Token}&refreshToken={result.Data.RefreshToken}");
            }

            return Redirect($"{redirectUrl}?error={Uri.EscapeDataString(result.Message ?? "login_failed")}");
        }
        catch (System.Exception ex)
        {
            return Redirect($"{redirectUrl}?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _authService.GetProfileAsync();
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _authService.UpdateProfileAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeMyPasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromQuery] string? password = null)
    {
        var result = await _authService.DeleteAccountAsync(password);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
