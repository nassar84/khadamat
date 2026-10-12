using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FcmNotification = FirebaseAdmin.Messaging.Notification;
using Google.Apis.Auth.OAuth2;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Khadamat.Infrastructure.Services;

public class FcmPushService : IFcmPushService
{
    private readonly KhadamatDbContext _context;
    private readonly ILogger<FcmPushService> _logger;
    private readonly IConfiguration _configuration;
    private static readonly object _lock = new();
    private static bool _isInitialized = false;

    public FcmPushService(KhadamatDbContext context, ILogger<FcmPushService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;

        EnsureFirebaseInitialized();
    }

    private void EnsureFirebaseInitialized()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            try
            {
                if (FirebaseApp.DefaultInstance != null)
                {
                    _isInitialized = true;
                    return;
                }

                // Check possible locations for firebase-service-account.json
                var possiblePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "src", "Khadamat.WebAPI", "firebase-service-account.json"),
                    _configuration["Firebase:ServiceAccountKeyPath"] ?? ""
                };

                string? validPath = possiblePaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));

                if (validPath != null)
                {
                    _logger.LogInformation("[FCM] Initializing FirebaseApp with credential file: {Path}", validPath);
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(validPath)
                    });
                    _isInitialized = true;
                    _logger.LogInformation("[FCM] FirebaseApp initialized successfully for project: khadamawy-prod");
                }
                else
                {
                    _logger.LogWarning("[FCM] firebase-service-account.json not found in searched locations. Push notifications will be skipped until configured.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCM] Failed to initialize FirebaseApp.");
            }
        }
    }

    public async Task<bool> RegisterDeviceTokenAsync(string userId, string token, string platform, string? deviceId = null, string? appVersion = null)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        userId = userId?.Trim() ?? string.Empty;

        try
        {
            var existing = await _context.UserDevices
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.DeviceToken == token);

            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(existing.UserId))
                {
                    existing.UserId = userId;
                }
                existing.Platform = platform ?? "android";
                existing.DeviceId = deviceId ?? existing.DeviceId;
                existing.AppVersion = appVersion ?? existing.AppVersion;
                existing.LastActiveAt = DateTime.UtcNow;
                existing.IsActive = true;
                existing.IsDeleted = false;
            }
            else
            {
                var newDevice = new UserDevice
                {
                    UserId = userId,
                    DeviceToken = token,
                    Platform = platform ?? "android",
                    DeviceId = deviceId,
                    AppVersion = appVersion,
                    LastActiveAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.UserDevices.AddAsync(newDevice);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[FCM] Registered device token successfully (User: '{UserId}', Platform: {Platform}, DeviceId: {DeviceId})", userId, platform, deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error registering device token for user '{UserId}'", userId);
            return false;
        }
    }

    public async Task<bool> UnregisterDeviceTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == token);

            if (existing != null)
            {
                existing.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("[FCM] Unregistered device token {Token}", token);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error unregistering token");
            return false;
        }
    }

    public async Task<bool> SendPushToUserAsync(string userId, string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("[FCM] Firebase not initialized. Skipping push to user {UserId}", userId);
            return false;
        }

        try
        {
            var tokens = await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive && !d.IsDeleted)
                .Select(d => d.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (tokens.Count == 0)
            {
                _logger.LogInformation("[FCM] No registered devices for user {UserId}. Skipping push.", userId);
                return false;
            }

            var sentCount = await SendBatchToTokensAsync(tokens, title, body, actionRoute, data);
            return sentCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error sending push to user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> SendPushToUsersAsync(IEnumerable<string> userIds, string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("[FCM] Firebase not initialized. Skipping push to users.");
            return 0;
        }

        try
        {
            var userList = userIds.Distinct().ToList();
            var tokens = await _context.UserDevices
                .Where(d => userList.Contains(d.UserId) && d.IsActive && !d.IsDeleted)
                .Select(d => d.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (tokens.Count == 0)
            {
                _logger.LogInformation("[FCM] No registered devices found for targeted users.");
                return 0;
            }

            return await SendBatchToTokensAsync(tokens, title, body, actionRoute, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error sending push to users list.");
            return 0;
        }
    }

    public async Task<int> SendBroadcastPushAsync(string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("[FCM] Firebase not initialized. Skipping broadcast push.");
            return 0;
        }

        try
        {
            var tokens = await _context.UserDevices
                .Where(d => d.IsActive && !d.IsDeleted)
                .Select(d => d.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (tokens.Count == 0)
            {
                _logger.LogInformation("[FCM] No active devices in database for broadcast.");
                return 0;
            }

            return await SendBatchToTokensAsync(tokens, title, body, actionRoute, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FCM] Error sending broadcast push.");
            return 0;
        }
    }

    private async Task<int> SendBatchToTokensAsync(List<string> tokens, string title, string body, string? actionRoute, Dictionary<string, string>? customData)
    {
        int successfulTotal = 0;
        var staleTokens = new List<string>();

        var dataPayload = new Dictionary<string, string>(customData ?? new Dictionary<string, string>())
        {
            ["title"] = title,
            ["body"] = body,
            ["action_route"] = actionRoute ?? "/",
            ["navigate_to"] = actionRoute ?? "/"
        };

        // Firebase multicast accepts max 500 tokens per call
        const int batchSize = 500;
        for (int i = 0; i < tokens.Count; i += batchSize)
        {
            var chunk = tokens.Skip(i).Take(batchSize).ToList();

            var message = new MulticastMessage
            {
                Tokens = chunk,
                Notification = new FcmNotification
                {
                    Title = title,
                    Body = body
                },
                Data = dataPayload,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "khadamawy_reminders_channel",
                        Sound = "default",
                        ClickAction = "OPEN_APP"
                    }
                }
            };

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                successfulTotal += response.SuccessCount;

                _logger.LogInformation("[FCM] Batch send result: {Success}/{Total} delivered.", response.SuccessCount, chunk.Count);

                for (int j = 0; j < response.Responses.Count; j++)
                {
                    var res = response.Responses[j];
                    if (!res.IsSuccess)
                    {
                        var errCode = res.Exception?.MessagingErrorCode;
                        if (errCode == MessagingErrorCode.Unregistered ||
                            errCode == MessagingErrorCode.InvalidArgument)
                        {
                            staleTokens.Add(chunk[j]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCM] Error sending multicast batch.");
            }
        }

        // Clean up stale tokens asynchronously
        if (staleTokens.Count > 0)
        {
            try
            {
                var staleDevices = await _context.UserDevices
                    .Where(d => staleTokens.Contains(d.DeviceToken))
                    .ToListAsync();

                foreach (var dev in staleDevices)
                {
                    dev.IsActive = false;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("[FCM] Deactivated {Count} stale/unregistered device tokens.", staleTokens.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FCM] Failed to deactivate stale tokens.");
            }
        }

        return successfulTotal;
    }
}
