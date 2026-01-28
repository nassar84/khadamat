using System.Net.Http.Json;
using Blazored.LocalStorage;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Khadamat.BlazorUI.State;
using Khadamat.BlazorUI.Helpers;

namespace Khadamat.BlazorUI.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;
    private readonly Khadamat.Shared.Interfaces.ISecureStorageService _secureStorage;
    private readonly AppState _appState;

    public AuthService(HttpClient httpClient,
                       AuthenticationStateProvider authenticationStateProvider,
                       ILocalStorageService localStorage,
                       Khadamat.Shared.Interfaces.ISecureStorageService secureStorage,
                       AppState appState)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
        _secureStorage = secureStorage;
        _appState = appState;
    }

    public async Task<AuthResponse?> Login(LoginRequest loginRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
            if (result?.Success == true && result.Data != null)
            {
                await _secureStorage.SaveAsync("authToken", result.Data.Token);
                await _secureStorage.SaveAsync("refreshToken", result.Data.RefreshToken);

                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Data.Token);
                return result.Data;
            }
        }
        return null;
    }

    public async Task<ApiResponse<AuthResponse>> Register(RegisterRequest registerRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", registerRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return result!;
    }

    public async Task Logout()
    {
        _secureStorage.Remove("authToken");
        _secureStorage.Remove("refreshToken");
        ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<ApiResponse<AuthResponse>> GetProfileAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<AuthResponse>>("api/v1/auth/profile");
        if (response?.Success == true && response.Data != null)
        {
            var p = response.Data;
            _appState.UpdateUserStatus(p.UserName, p.Roles.FirstOrDefault() ?? "User", p.IsProvider, DefaultImages.GetUserAvatar(p.UserName, p.Gender, p.ImageUrl));
            _appState.CityId = p.CityId;
            _appState.GovernorateId = p.GovernorateId;
            _appState.PhoneNumber = p.PhoneNumber;
        }
        return response!;
    }

    public async Task<ApiResponse<bool>> UpdateProfile(UpdateProfileRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync("api/v1/auth/profile", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        return result!;
    }

    public async Task<ApiResponse<bool>> ChangePassword(ChangeMyPasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/auth/change-password", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        return result!;
    }

    public async Task<bool> LoginWithToken(string token, string refreshToken)
    {
        await _localStorage.SetItemAsync("authToken", token);
        await _localStorage.SetItemAsync("refreshToken", refreshToken);

        ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(token);
        return true;
    }
}
