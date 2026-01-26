using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using System.Threading.Tasks;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly Microsoft.AspNetCore.Identity.SignInManager<Khadamat.Infrastructure.Identity.ApplicationUser> _signInManager;

    public AuthController(IAuthService authService, Microsoft.AspNetCore.Identity.SignInManager<Khadamat.Infrastructure.Identity.ApplicationUser> signInManager)
    {
        _authService = authService;
        _signInManager = signInManager;
    }

    [HttpGet("external-login")]
    public IActionResult ExternalLogin(string provider, string redirectUrl)
    {
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, 
            Url.Action("ExternalLoginCallback", new { redirectUrl }));
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string redirectUrl, string? remoteError = null)
    {
        if (remoteError != null)
        {
            return Redirect($"{redirectUrl}?error={remoteError}");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return Redirect($"{redirectUrl}?error=failed_to_get_external_login_info");
        }

        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);
        var provider = info.LoginProvider;
        var providerUserId = info.ProviderKey;

        var result = await _authService.ExternalLoginCallbackAsync(email!, name!, provider, providerUserId);

        if (result.Success)
        {
            return Redirect($"{redirectUrl}?token={result.Data.Token}&refreshToken={result.Data.RefreshToken}");
        }

        return Redirect($"{redirectUrl}?error={result.Message}");
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
}
