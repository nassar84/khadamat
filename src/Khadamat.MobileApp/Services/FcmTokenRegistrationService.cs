using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Khadamat.MobileApp.Services;

/// <summary>
/// Handles FCM device token registration with the Khadamat backend API.
/// 
/// Flow:
///   1. Firebase SDK generates token via KhadamatFirebaseMessagingService.OnNewToken()
///   2. Token is stored via Preferences.Default.Set("fcm_device_token", token)
///   3. After login, call FcmTokenRegistrationService.RegisterTokenAsync()
///   4. This sends the token to the API endpoint: POST /api/notifications/register-device
/// 
/// Register in MauiProgram.cs:
///   builder.Services.AddSingleton<FcmTokenRegistrationService>();
/// </summary>
public class FcmTokenRegistrationService
{
    private readonly HttpClient _http;
    private readonly ILogger<FcmTokenRegistrationService> _logger;

    public FcmTokenRegistrationService(HttpClient http, ILogger<FcmTokenRegistrationService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Sends the stored FCM device token to the backend API.
    /// Call this after the user logs in successfully.
    /// </summary>
    public async Task RegisterTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("fcm_device_token");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("[FCM] No device token found to register.");
                return;
            }

            var platform = DeviceInfo.Platform == DevicePlatform.Android ? "android" : "ios";

            var payload = new
            {
                Token = token,
                Platform = platform,
                DeviceId = DeviceInfo.Idiom.ToString(),
                AppVersion = AppInfo.VersionString
            };

            var response = await _http.PostAsJsonAsync("/api/notifications/register-device", payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[FCM] Device token registered successfully.");
                await SecureStorage.Default.SetAsync("fcm_token_registered", "true");
            }
            else
            {
                _logger.LogWarning("[FCM] Failed to register device token: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error registering device token.");
        }
    }

    /// <summary>
    /// Unregisters the device token from the API (e.g., on logout).
    /// </summary>
    public async Task UnregisterTokenAsync()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("fcm_device_token");
            if (string.IsNullOrEmpty(token)) return;

            await _http.DeleteAsync($"/api/notifications/unregister-device?token={Uri.EscapeDataString(token)}");
            await SecureStorage.Default.SetAsync("fcm_token_registered", "false");

            _logger.LogInformation("[FCM] Device token unregistered.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error unregistering device token.");
        }
    }
}
