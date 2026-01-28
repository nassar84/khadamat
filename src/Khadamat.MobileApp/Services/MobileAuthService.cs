using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.BlazorUI.Services;
using Khadamat.BlazorUI.Services.Auth;
using Microsoft.Maui.Authentication;

namespace Khadamat.MobileApp.Services
{
    public interface IMobileAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginWithExternalProvider(string provider);
    }

    public class MobileAuthService : IMobileAuthService
    {
        private readonly ApiClient _apiClient;
        private readonly IAuthService _authService;

        // Note: For a real app, these should be in a secure config or fetched from backend
        private const string GoogleClientId = "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com";
        private const string FacebookAppId = "YOUR_FACEBOOK_APP_ID";
        private const string CallbackScheme = "khadamat";

        public MobileAuthService(ApiClient apiClient, IAuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        public async Task<ApiResponse<AuthResponse>> LoginWithExternalProvider(string provider)
        {
            try
            {
                string authUrl = "";
                if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    // Authorization Code Flow with PKCE is better, but WebAuthenticator can handle simple implicit/code flow
                    // Here we construct a direct Google OAuth URL
                    authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                              $"client_id={GoogleClientId}&" +
                              $"response_type=id_token token&" + // Request both for simplicity or just id_token
                              $"scope=openid%20email%20profile&" +
                              $"redirect_uri={CallbackScheme}://auth-callback&" +
                              $"nonce={Guid.NewGuid():N}";
                }
                else if (provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
                {
                    authUrl = $"https://www.facebook.com/v14.0/dialog/oauth?" +
                              $"client_id={FacebookAppId}&" +
                              $"response_type=token&" +
                              $"scope=email,public_profile&" +
                              $"redirect_uri={CallbackScheme}://auth-callback";
                }

                var result = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri($"{CallbackScheme}://"));

                // Depending on the provider, the token might be in "id_token" or "access_token"
                string token = result.Properties.ContainsKey("id_token") 
                    ? result.Properties["id_token"] 
                    : result.AccessToken;

                if (string.IsNullOrEmpty(token))
                    return ApiResponse<AuthResponse>.Fail("لم يتم الحصول على توكن من مزود الخدمة");

                // Send the token to OUR Web API for validation and JWT generation
                var exchangeRequest = new ExternalTokenLoginRequest
                {
                    Provider = provider,
                    Token = token
                };

                // We need to call the new API endpoint
                var apiResponse = await _apiClient.PostAsync<ApiResponse<AuthResponse>>("api/v1/auth/external-token-login", exchangeRequest);
                
                if (apiResponse != null && apiResponse.Success)
                {
                    // Successfully logged in, now save the token in our AppState/Storage
                    await _authService.LoginWithToken(apiResponse.Data.Token, apiResponse.Data.RefreshToken);
                    return apiResponse;
                }

                return apiResponse ?? ApiResponse<AuthResponse>.Fail("فشل التحقق من التوكن مع الخادم");
            }
            catch (OperationCanceledException)
            {
                return ApiResponse<AuthResponse>.Fail("تم إلغاء عملية تسجيل الدخول");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"حدث خطأ: {ex.Message}");
            }
        }
    }
}
