using System.Net.Http.Json;
using Blazored.LocalStorage;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Khadamat.BlazorUI.State;
using Khadamat.BlazorUI.Helpers;
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorage;
    private readonly Khadamat.Shared.Interfaces.ISecureStorageService _secureStorage;
    private readonly AppState _appState;
    private readonly IJSRuntime _js;

    public AuthService(HttpClient httpClient,
                       AuthenticationStateProvider authenticationStateProvider,
                       ILocalStorageService localStorage,
                       Khadamat.Shared.Interfaces.ISecureStorageService secureStorage,
                       AppState appState,
                       IJSRuntime js)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
        _secureStorage = secureStorage;
        _appState = appState;
        _js = js;
    }

    private async Task NotifyNativeApp(string message, string? data = null)
    {
        if (!_appState.IsNativeApp) return;

        try
        {
            var url = $"khadamat://auth/{message}";
            if (!string.IsNullOrEmpty(data))
            {
                url += $"?data={Uri.EscapeDataString(data)}";
            }
            await _js.InvokeVoidAsync("eval", $"window.location.href = '{url}'");
        }
        catch { /* Fallback or ignore if not in native webview */ }
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

                _appState.UserToken = result.Data.Token;

                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(result.Data.Token);
                await NotifyNativeApp("auth_success");
                
                // Fetch full profile immediately to populate AppState before returning
                await GetProfileAsync(); 
                
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

    public Task Logout()
    {
        _secureStorage.Remove("authToken");
        _secureStorage.Remove("refreshToken");
        
        _appState.UserToken = null;

        ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _ = NotifyNativeApp("auth_logout");
        return Task.CompletedTask;
    }

    public async Task<ApiResponse<AuthResponse>> GetProfileAsync()
    {
        try 
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AuthResponse>>("api/v1/auth/profile");
            if (response?.Success == true && response.Data != null)
            {
                var p = response.Data;
                _appState.UpdateUserStatus(p.UserName, p.Roles.FirstOrDefault() ?? "User", p.IsProvider, DefaultImages.GetUserAvatar(p.UserName, p.Gender, p.ImageUrl), p.Id);
                _appState.CityId = p.CityId;
                _appState.GovernorateId = p.GovernorateId;
                _appState.PhoneNumber = p.PhoneNumber;

                // Notify native app with full profile data
                var roles = string.Join(",", p.Roles);
                var is_admin = p.Roles.Any(r => r == "SystemAdmin" || r == "SuperAdmin").ToString().ToLower();
                var nativeData = $"name={p.UserName}&image={p.ImageUrl}&is_admin={is_admin}&is_provider={p.IsProvider.ToString().ToLower()}";
                _ = NotifyNativeApp("auth_success", nativeData);
            }
            return response!;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Logout();
            return new ApiResponse<AuthResponse> { Success = false, Message = "Session expired. Please login again." };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting profile in AuthService: {ex.Message}");
            return new ApiResponse<AuthResponse> { Success = false, Message = "Failed to load profile" };
        }
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
        await _secureStorage.SaveAsync("authToken", token);
        await _secureStorage.SaveAsync("refreshToken", refreshToken);

        _appState.UserToken = token;

        ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(token);
        await NotifyNativeApp("auth_success");
        
        // Fetch full profile immediately
        await GetProfileAsync();

        return true;
    }

    public async Task InitializeAsync()
    {
        var token = await _secureStorage.GetAsync("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _appState.UserToken = token;
            
            // المكون الأصلي (GetAuthenticationStateAsync) يتولى بالفعل بناء Auth State في البداية.
            // لا تقم باستدعاء NotifyAuthenticationStateChanged هنا أبداً!
            // هذا كان يُطلق Full-App Re-render في نفس لحظة First Render مما يُسبب خطأ:
            // "No element is currently associated with component"
            
            await NotifyNativeApp("auth_success");
            
            // Try to load profile in background
            _ = GetProfileAsync();
        }
    }
}
