using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;

namespace Khadamat.BlazorUI.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse?> Login(LoginRequest loginRequest);
    Task<Khadamat.Application.Common.Models.ApiResponse<AuthResponse>> Register(RegisterRequest registerRequest);
    Task Logout();
    Task<Khadamat.Application.Common.Models.ApiResponse<AuthResponse>> GetProfileAsync();
    Task<Khadamat.Application.Common.Models.ApiResponse<bool>> UpdateProfile(UpdateProfileRequest request);
    Task<Khadamat.Application.Common.Models.ApiResponse<bool>> ChangePassword(ChangeMyPasswordRequest request);
}
