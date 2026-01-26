using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using System.Threading.Tasks;

namespace Khadamat.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<AuthResponse>> GetProfileAsync();
    Task<ApiResponse<bool>> UpdateProfileAsync(UpdateProfileRequest request);
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangeMyPasswordRequest request);
    Task<bool> SetUserIsProviderAsync(string userId, bool isProvider);
    Task<ApiResponse<AuthResponse>> ExternalLoginCallbackAsync(string email, string name, string provider, string providerUserId);
}
