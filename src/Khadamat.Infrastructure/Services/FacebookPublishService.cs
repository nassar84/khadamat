using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Khadamat.Infrastructure.Services;

public class FacebookPublishService : IFacebookPublishService
{
    private readonly KhadamatDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FacebookPublishService> _logger;
    private readonly IDataProtector _protector;
    private const string GraphApiVersion = "v19.0";
    private const string GraphBaseUrl = $"https://graph.facebook.com/{GraphApiVersion}/";

    public FacebookPublishService(
        KhadamatDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<FacebookPublishService> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _protector = dataProtectionProvider.CreateProtector("Khadamat.FacebookPageTokenProtector.v1");
    }

    public async Task<FacebookIntegrationStatusDto> GetIntegrationStatusAsync()
    {
        var config = await GetOrCreateConfigAsync();
        var appId = _configuration["Authentication:Facebook:AppId"] ?? config.AppId;
        var maskedAppId = MaskString(appId);

        var totalPublished = await _context.FacebookPublishLogs.CountAsync(l => l.Status == "Published");
        var pendingReview = await _context.FacebookPublishLogs.CountAsync(l => l.Status == "PendingReview");
        var failed = await _context.FacebookPublishLogs.CountAsync(l => l.Status == "Failed");

        var hasManagePerms = !string.IsNullOrEmpty(config.PermissionsGranted) && 
                             (config.PermissionsGranted.Contains("pages_manage_posts") || 
                              config.PermissionsGranted.Contains("CREATE_CONTENT"));

        return new FacebookIntegrationStatusDto
        {
            IsConnected = config.IsConnected && !string.IsNullOrEmpty(config.PageId),
            MaskedAppId = maskedAppId,
            PageName = config.PageName,
            PageUsername = config.PageUsername,
            PageUrl = config.PageUrl,
            PageId = config.PageId,
            ConnectedAt = config.ConnectedAt,
            ConnectedByAdminName = config.ConnectedByAdminName,
            PermissionsGranted = config.PermissionsGranted,
            HasManagePostsPermission = hasManagePerms,
            PublishMode = config.PublishMode,
            AutoPublishOnApproval = config.AutoPublishOnApproval,
            LastConnectionCheck = config.LastConnectionCheck,
            LastConnectionSuccess = config.LastConnectionSuccess,
            LastConnectionError = config.LastConnectionError,
            TotalPublishedPosts = totalPublished,
            PendingReviewCount = pendingReview,
            FailedCount = failed
        };
    }

    public async Task<FacebookOAuthInitResponseDto> InitiateAdminAuthorizationAsync(string returnUrl)
    {
        var config = await GetOrCreateConfigAsync();
        var appId = _configuration["Authentication:Facebook:AppId"] ?? config.AppId;

        // Generate cryptographic state to prevent CSRF
        var stateBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(stateBytes);
        }
        var state = Convert.ToBase64String(stateBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");

        // Cache or store state temporarily in database
        config.LastConnectionError = $"state:{state}:{DateTime.UtcNow.AddMinutes(15):o}:{returnUrl}";
        await _context.SaveChangesAsync();

        var scopes = "pages_show_list,pages_read_engagement,pages_manage_posts";
        var oauthUrl = $"https://www.facebook.com/{GraphApiVersion}/dialog/oauth?" +
                       $"client_id={Uri.EscapeDataString(appId)}" +
                       $"&redirect_uri={Uri.EscapeDataString(returnUrl)}" +
                       $"&scope={Uri.EscapeDataString(scopes)}" +
                       $"&response_type=code" +
                       $"&state={Uri.EscapeDataString(state)}";

        return new FacebookOAuthInitResponseDto
        {
            OAuthUrl = oauthUrl,
            State = state
        };
    }

    public async Task<ApiResponse<List<FacebookManagedPageDto>>> ProcessOAuthCallbackAsync(string code, string state)
    {
        try
        {
            var config = await GetOrCreateConfigAsync();
            var appId = _configuration["Authentication:Facebook:AppId"] ?? config.AppId;
            var appSecret = _configuration["Authentication:Facebook:AppSecret"];

            if (string.IsNullOrEmpty(appSecret))
            {
                return ApiResponse<List<FacebookManagedPageDto>>.Fail("AppSecret غير مهيأ في الإعدادات");
            }

            // Verify state
            if (!string.IsNullOrEmpty(config.LastConnectionError) && config.LastConnectionError.StartsWith("state:"))
            {
                var parts = config.LastConnectionError.Split(':');
                if (parts.Length >= 4)
                {
                    var savedState = parts[1];
                    var expiresAt = DateTime.Parse(parts[2]);
                    var returnUrl = string.Join(":", parts.Skip(3));

                    if (DateTime.UtcNow > expiresAt || savedState != state)
                    {
                        return ApiResponse<List<FacebookManagedPageDto>>.Fail("جلسة التفويض غير صالحة أو منتهية الصلاحية (CSRF Verification Failed)");
                    }

                    var httpClient = _httpClientFactory.CreateClient();

                    // 1. Exchange authorization code for short-lived User Access Token
                    var tokenExchangeUrl = $"{GraphBaseUrl}oauth/access_token?" +
                                           $"client_id={appId}" +
                                           $"&client_secret={appSecret}" +
                                           $"&redirect_uri={Uri.EscapeDataString(returnUrl)}" +
                                           $"&code={code}";

                    var tokenResponse = await httpClient.GetFromJsonAsync<FbTokenResponse>(tokenExchangeUrl);
                    if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
                    {
                        return ApiResponse<List<FacebookManagedPageDto>>.Fail("فشل استبدال كود التفويض برمز وصول المستخدم من Meta");
                    }

                    // 2. Exchange short-lived token for Long-Lived User Access Token (60 days)
                    var longLivedTokenUrl = $"{GraphBaseUrl}oauth/access_token?" +
                                            $"grant_type=fb_exchange_token" +
                                            $"&client_id={appId}" +
                                            $"&client_secret={appSecret}" +
                                            $"&fb_exchange_token={tokenResponse.access_token}";

                    var longLivedResponse = await httpClient.GetFromJsonAsync<FbTokenResponse>(longLivedTokenUrl);
                    var effectiveUserToken = longLivedResponse?.access_token ?? tokenResponse.access_token;

                    // 3. Query GET /me/accounts with Long-Lived Token to get non-expiring Page Access Tokens
                    var accountsUrl = $"{GraphBaseUrl}me/accounts?" +
                                      $"fields=id,name,username,link,category,access_token,tasks" +
                                      $"&access_token={effectiveUserToken}";

                    var accountsResponse = await httpClient.GetFromJsonAsync<FbAccountsResponse>(accountsUrl);
                    if (accountsResponse == null || accountsResponse.data == null || accountsResponse.data.Count == 0)
                    {
                        return ApiResponse<List<FacebookManagedPageDto>>.Fail("لم يتم العثور على أي صفحات فيسبوك تابعة لهذا الحساب بصلاحيات الإدارة");
                    }

                    var pages = new List<FacebookManagedPageDto>();
                    foreach (var page in accountsResponse.data)
                    {
                        var canCreateContent = page.tasks != null && 
                            (page.tasks.Contains("CREATE_CONTENT") || page.tasks.Contains("MANAGE"));

                        var isOfficial = (!string.IsNullOrEmpty(page.username) && page.username.Equals("KhadamawyApp", StringComparison.OrdinalIgnoreCase)) ||
                                         (!string.IsNullOrEmpty(page.name) && page.name.Contains("خدماو", StringComparison.OrdinalIgnoreCase));

                        pages.Add(new FacebookManagedPageDto
                        {
                            Id = page.id,
                            Name = page.name,
                            Username = page.username,
                            Category = page.category,
                            Link = page.link,
                            Tasks = page.tasks ?? new List<string>(),
                            CanCreateContent = canCreateContent,
                            IsOfficialKhadamawyPage = isOfficial,
                            PageAccessToken = ProtectToken(page.access_token) // Return protected token so raw token is never passed openly
                        });
                    }

                    return ApiResponse<List<FacebookManagedPageDto>>.Succeed(pages);
                }
            }

            return ApiResponse<List<FacebookManagedPageDto>>.Fail("معلومات جلسة التفويض مفقودة");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Facebook OAuth callback");
            return ApiResponse<List<FacebookManagedPageDto>>.Fail("خطأ أثناء استكمال التفويض مع Meta: " + ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ConnectPageAsync(FacebookConnectPageRequestDto request, string adminUserId, string adminUserName)
    {
        try
        {
            if (string.IsNullOrEmpty(request.PageId) || string.IsNullOrEmpty(request.PageAccessToken))
            {
                return ApiResponse<bool>.Fail("بيانات الصفحة أو رمز الوصول غير مكتملة");
            }

            // Unprotect token
            var rawToken = UnprotectToken(request.PageAccessToken);
            if (string.IsNullOrEmpty(rawToken))
            {
                return ApiResponse<bool>.Fail("رمز الوصول غير صالح");
            }

            // Verify with Graph API
            var httpClient = _httpClientFactory.CreateClient();
            var testUrl = $"{GraphBaseUrl}{request.PageId}?fields=id,name,username,link&access_token={rawToken}";
            var pageDetails = await httpClient.GetFromJsonAsync<FbPageDetails>(testUrl);

            if (pageDetails == null || string.IsNullOrEmpty(pageDetails.id))
            {
                return ApiResponse<bool>.Fail("فشل التحقق من الصفحة عبر Meta Graph API");
            }

            var config = await GetOrCreateConfigAsync();
            config.PageId = pageDetails.id;
            config.PageName = pageDetails.name ?? request.PageName;
            config.PageUsername = pageDetails.username ?? request.PageUsername ?? "KhadamawyApp";
            config.PageUrl = !string.IsNullOrEmpty(pageDetails.link) ? pageDetails.link : "https://www.facebook.com/khadamawyApp/";
            config.ProtectedPageAccessToken = ProtectToken(rawToken);
            config.IsConnected = true;
            config.ConnectedAt = DateTime.UtcNow;
            config.ConnectedByAdminUserId = adminUserId;
            config.ConnectedByAdminName = adminUserName;
            config.PageTasks = request.Tasks ?? "CREATE_CONTENT, MANAGE";
            config.PermissionsGranted = request.Permissions ?? "pages_show_list,pages_read_engagement,pages_manage_posts";
            config.LastConnectionCheck = DateTime.UtcNow;
            config.LastConnectionSuccess = true;
            config.LastConnectionError = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Facebook Official Page '{PageName}' (ID: {PageId}) successfully connected by admin {Admin}", 
                config.PageName, config.PageId, adminUserName);

            return ApiResponse<bool>.Succeed(true, "تم ربط صفحة خدماوي الرسمية بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting Facebook page");
            return ApiResponse<bool>.Fail("حدث خطأ أثناء ربط الصفحة: " + ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DisconnectPageAsync(string adminUserId)
    {
        try
        {
            var config = await GetOrCreateConfigAsync();
            config.IsConnected = false;
            config.ProtectedPageAccessToken = null;
            config.PageId = null;
            config.LastConnectionCheck = DateTime.UtcNow;
            config.LastConnectionSuccess = false;
            config.LastConnectionError = "تم إلغاء ربط الصفحة يدوياً بواسطة الإدارة";

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Succeed(true, "تم إلغاء ربط صفحة فيسبوك بنجاح");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting Facebook page");
            return ApiResponse<bool>.Fail("فشل إلغاء ربط الصفحة: " + ex.Message);
        }
    }

    public async Task<FacebookConnectionTestResultDto> ValidatePageConnectionAsync()
    {
        var config = await GetOrCreateConfigAsync();
        var result = new FacebookConnectionTestResultDto
        {
            PageId = config.PageId,
            PageName = config.PageName,
            CheckedAt = DateTime.UtcNow
        };

        if (!config.IsConnected || string.IsNullOrEmpty(config.PageId) || string.IsNullOrEmpty(config.ProtectedPageAccessToken))
        {
            result.Success = false;
            result.ErrorMessage = "الصفحة غير مربوطة حالياً. يرجى الضغط على زر ربط الصفحة لتفويض المسؤول.";
            return result;
        }

        try
        {
            var rawToken = UnprotectToken(config.ProtectedPageAccessToken);
            if (string.IsNullOrEmpty(rawToken))
            {
                result.Success = false;
                result.ErrorMessage = "تعذر فك تشفير رمز وصول الصفحة. يرجى إعادة التفويض.";
                return result;
            }

            var httpClient = _httpClientFactory.CreateClient();
            var checkUrl = $"{GraphBaseUrl}{config.PageId}?fields=id,name,username,link&access_token={rawToken}";
            var response = await httpClient.GetAsync(checkUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errContent = await response.Content.ReadAsStringAsync();
                result.Success = false;
                result.ErrorMessage = $"فشل الاتصال بـ Meta API ({response.StatusCode}): {errContent}";
                
                config.LastConnectionCheck = DateTime.UtcNow;
                config.LastConnectionSuccess = false;
                config.LastConnectionError = result.ErrorMessage;
                await _context.SaveChangesAsync();
                return result;
            }

            var pageDetails = await response.Content.ReadFromJsonAsync<FbPageDetails>();
            result.Success = true;
            result.PageId = pageDetails?.id ?? config.PageId;
            result.PageName = pageDetails?.name ?? config.PageName;
            result.CanPublish = true;
            result.Permissions = config.PermissionsGranted ?? "pages_manage_posts,pages_read_engagement,pages_show_list";

            config.LastConnectionCheck = DateTime.UtcNow;
            config.LastConnectionSuccess = true;
            config.LastConnectionError = null;
            await _context.SaveChangesAsync();

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = "حدث استثناء أثناء فحص الاتصال: " + ex.Message;
            return result;
        }
    }

    public async Task<FacebookConnectionTestResultDto> ValidatePublishingPermissionsAsync()
    {
        return await ValidatePageConnectionAsync();
    }

    public async Task<ApiResponse<FacebookPostResultDto>> PublishTextPostAsync(string message, string? adminUserId = null, string? adminUserName = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return ApiResponse<FacebookPostResultDto>.Fail("نص المنشور مطلوب ولا يمكن تركه فارغاً");
        }

        var log = new FacebookPublishLog
        {
            PostType = "TextPost",
            PostText = message,
            Status = "InProgress",
            AttemptedAt = DateTime.UtcNow,
            AdminUserId = adminUserId,
            AdminUserName = adminUserName
        };
        await _context.FacebookPublishLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        try
        {
            var config = await GetOrCreateConfigAsync();
            if (!config.IsConnected || string.IsNullOrEmpty(config.PageId) || string.IsNullOrEmpty(config.ProtectedPageAccessToken))
            {
                log.Status = "Failed";
                log.ErrorMessage = "صفحة فيسبوك غير مربوطة في لوحة الإدارة";
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var rawToken = UnprotectToken(config.ProtectedPageAccessToken);
            var httpClient = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
            {
                { "message", message },
                { "access_token", rawToken }
            };

            var postUrl = $"{GraphBaseUrl}{config.PageId}/feed";
            var response = await httpClient.PostAsync(postUrl, new FormUrlEncodedContent(form));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.Status = "Failed";
                log.ErrorMessage = $"Meta API Error ({response.StatusCode}): {responseBody}";
                log.RetryCount++;
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var publishRes = JsonSerializer.Deserialize<FbPublishResponse>(responseBody);
            var postId = publishRes?.id ?? string.Empty;

            log.FacebookPageId = config.PageId;
            log.FacebookPostId = postId;
            log.PostUrl = ConstructPostUrl(config.PageId, config.PageUsername, postId);
            log.Status = "Published";
            log.PublishedAt = DateTime.UtcNow;
            log.ErrorMessage = null;
            await _context.SaveChangesAsync();

            return ApiResponse<FacebookPostResultDto>.Succeed(new FacebookPostResultDto
            {
                PostId = postId,
                PostUrl = log.PostUrl,
                PublishedAt = DateTime.UtcNow,
                Success = true,
                Message = "تم نشر المنشور على صفحة فيسبوك الرسمية بنجاح"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing text post to Facebook");
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            return ApiResponse<FacebookPostResultDto>.Fail("خطأ أثناء النشر: " + ex.Message);
        }
    }

    public async Task<ApiResponse<FacebookPostResultDto>> PublishPhotoPostAsync(string caption, string photoUrl, string? adminUserId = null, string? adminUserName = null)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return ApiResponse<FacebookPostResultDto>.Fail("رابط الصورة مطلوب لنشر صورة");
        }

        var log = new FacebookPublishLog
        {
            PostType = "PhotoPost",
            PostText = caption,
            ImageUrl = photoUrl,
            Status = "InProgress",
            AttemptedAt = DateTime.UtcNow,
            AdminUserId = adminUserId,
            AdminUserName = adminUserName
        };
        await _context.FacebookPublishLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        try
        {
            var config = await GetOrCreateConfigAsync();
            if (!config.IsConnected || string.IsNullOrEmpty(config.PageId) || string.IsNullOrEmpty(config.ProtectedPageAccessToken))
            {
                log.Status = "Failed";
                log.ErrorMessage = "صفحة فيسبوك غير مربوطة في لوحة الإدارة";
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var rawToken = UnprotectToken(config.ProtectedPageAccessToken);
            var httpClient = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
            {
                { "caption", caption ?? string.Empty },
                { "url", photoUrl },
                { "access_token", rawToken }
            };

            var postUrl = $"{GraphBaseUrl}{config.PageId}/photos";
            var response = await httpClient.PostAsync(postUrl, new FormUrlEncodedContent(form));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.Status = "Failed";
                log.ErrorMessage = $"Meta API Error ({response.StatusCode}): {responseBody}";
                log.RetryCount++;
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var publishRes = JsonSerializer.Deserialize<FbPublishResponse>(responseBody);
            var postId = publishRes?.id ?? publishRes?.post_id ?? string.Empty;

            log.FacebookPageId = config.PageId;
            log.FacebookPostId = postId;
            log.PostUrl = ConstructPostUrl(config.PageId, config.PageUsername, postId);
            log.Status = "Published";
            log.PublishedAt = DateTime.UtcNow;
            log.ErrorMessage = null;
            await _context.SaveChangesAsync();

            return ApiResponse<FacebookPostResultDto>.Succeed(new FacebookPostResultDto
            {
                PostId = postId,
                PostUrl = log.PostUrl,
                PublishedAt = DateTime.UtcNow,
                Success = true,
                Message = "تم نشر الصورة على صفحة فيسبوك بنجاح"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing photo post to Facebook");
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            return ApiResponse<FacebookPostResultDto>.Fail("خطأ أثناء النشر: " + ex.Message);
        }
    }

    public async Task<ApiResponse<FacebookPostResultDto>> PublishServiceCardAsync(int serviceId, string? customCaption = null, string? adminUserId = null, string? adminUserName = null)
    {
        // 1. Check deduplication: check if already published
        var existingPublished = await _context.FacebookPublishLogs
            .FirstOrDefaultAsync(l => l.ServiceId == serviceId && l.Status == "Published");

        if (existingPublished != null)
        {
            return ApiResponse<FacebookPostResultDto>.Fail($"تم نشر كارت هذه الخدمة مسبقاً على الصفحة برقم منشور ({existingPublished.FacebookPostId})");
        }

        var service = await _context.Services
            .Include(s => s.City)
            .Include(s => s.Category)
            .Include(s => s.ProviderProfile)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            return ApiResponse<FacebookPostResultDto>.Fail("الخدمة المطلوبة غير موجودة");
        }

        // Generate caption if not provided
        var postCaption = !string.IsNullOrWhiteSpace(customCaption) 
            ? customCaption 
            : await BuildCelebratoryCaptionAsync(service);

        // Build Card Image URL
        var baseSiteUrl = _configuration["BaseUrl"] ?? "https://khadamawy.eis-dev.com";
        var cardImageUrl = $"{baseSiteUrl.TrimEnd('/')}/share/service/{service.Id}/og-image";

        var log = new FacebookPublishLog
        {
            ServiceId = service.Id,
            PostType = "ServiceCard",
            PostText = postCaption,
            ImageUrl = cardImageUrl,
            Status = "InProgress",
            AttemptedAt = DateTime.UtcNow,
            AdminUserId = adminUserId,
            AdminUserName = adminUserName
        };
        await _context.FacebookPublishLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        try
        {
            var config = await GetOrCreateConfigAsync();
            if (!config.IsConnected || string.IsNullOrEmpty(config.PageId) || string.IsNullOrEmpty(config.ProtectedPageAccessToken))
            {
                log.Status = "Failed";
                log.ErrorMessage = "صفحة فيسبوك غير مربوطة في لوحة الإدارة";
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var rawToken = UnprotectToken(config.ProtectedPageAccessToken);
            var httpClient = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
            {
                { "caption", postCaption },
                { "url", cardImageUrl },
                { "access_token", rawToken }
            };

            var postUrl = $"{GraphBaseUrl}{config.PageId}/photos";
            var response = await httpClient.PostAsync(postUrl, new FormUrlEncodedContent(form));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                log.Status = "Failed";
                log.ErrorMessage = $"Meta API Error ({response.StatusCode}): {responseBody}";
                log.RetryCount++;
                await _context.SaveChangesAsync();
                return ApiResponse<FacebookPostResultDto>.Fail(log.ErrorMessage);
            }

            var publishRes = JsonSerializer.Deserialize<FbPublishResponse>(responseBody);
            var postId = publishRes?.id ?? publishRes?.post_id ?? string.Empty;

            log.FacebookPageId = config.PageId;
            log.FacebookPostId = postId;
            log.PostUrl = ConstructPostUrl(config.PageId, config.PageUsername, postId);
            log.Status = "Published";
            log.PublishedAt = DateTime.UtcNow;
            log.ErrorMessage = null;
            await _context.SaveChangesAsync();

            return ApiResponse<FacebookPostResultDto>.Succeed(new FacebookPostResultDto
            {
                PostId = postId,
                PostUrl = log.PostUrl,
                PublishedAt = DateTime.UtcNow,
                Success = true,
                Message = $"تم نشر كارت خدمة \"{service.Name}\" بنجاح على صفحة خدماوي الرسمية"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing service card to Facebook");
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();
            return ApiResponse<FacebookPostResultDto>.Fail("خطأ أثناء النشر: " + ex.Message);
        }
    }

    public async Task<ApiResponse<string>> PrepareServiceCardPostTextAsync(int serviceId)
    {
        var service = await _context.Services
            .Include(s => s.City)
            .Include(s => s.Category)
            .Include(s => s.ProviderProfile)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null) return ApiResponse<string>.Fail("الخدمة غير موجودة");

        var text = await BuildCelebratoryCaptionAsync(service);
        return ApiResponse<string>.Succeed(text);
    }

    public async Task<List<FacebookPublishLogDto>> GetPublishLogsAsync(int? serviceId = null, string? status = null, int take = 50)
    {
        var query = _context.FacebookPublishLogs
            .Include(l => l.Service)
            .AsNoTracking();

        if (serviceId.HasValue && serviceId.Value > 0)
        {
            query = query.Where(l => l.ServiceId == serviceId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(l => l.Status == status);
        }

        var logs = await query.OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync();

        return logs.Select(l => new FacebookPublishLogDto
        {
            Id = l.Id,
            ServiceId = l.ServiceId,
            ServiceName = l.Service?.Name,
            PostType = l.PostType,
            FacebookPostId = l.FacebookPostId,
            PostUrl = l.PostUrl,
            PostText = l.PostText,
            ImageUrl = l.ImageUrl,
            Status = l.Status,
            AttemptedAt = l.AttemptedAt,
            PublishedAt = l.PublishedAt,
            RetryCount = l.RetryCount,
            ErrorMessage = l.ErrorMessage,
            AdminUserName = l.AdminUserName,
            AdminNotes = l.AdminNotes
        }).ToList();
    }

    public async Task<ApiResponse<bool>> RetryPublishAsync(int logId, string adminUserId)
    {
        var log = await _context.FacebookPublishLogs.FindAsync(logId);
        if (log == null) return ApiResponse<bool>.Fail("السجل غير موجود");

        if (log.ServiceId.HasValue)
        {
            var res = await PublishServiceCardAsync(log.ServiceId.Value, log.PostText, adminUserId, log.AdminUserName);
            return ApiResponse<bool>.Succeed(res.Success, res.Message);
        }
        else if (log.PostType == "PhotoPost" && !string.IsNullOrEmpty(log.ImageUrl))
        {
            var res = await PublishPhotoPostAsync(log.PostText, log.ImageUrl, adminUserId, log.AdminUserName);
            return ApiResponse<bool>.Succeed(res.Success, res.Message);
        }
        else
        {
            var res = await PublishTextPostAsync(log.PostText, adminUserId, log.AdminUserName);
            return ApiResponse<bool>.Succeed(res.Success, res.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelPublishAsync(int logId, string adminUserId)
    {
        var log = await _context.FacebookPublishLogs.FindAsync(logId);
        if (log == null) return ApiResponse<bool>.Fail("السجل غير موجود");

        if (log.Status == "Published")
        {
            return ApiResponse<bool>.Fail("لا يمكن إلغاء منشور تم نشره بالفعل على فيسبوك من هنا");
        }

        log.Status = "Cancelled";
        log.AdminNotes = $"تم الإلغاء بواسطة المستخدم {adminUserId} بتاريخ {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم إلغاء المنشور بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdatePublishModeAsync(string mode, bool autoPublishOnApproval)
    {
        var config = await GetOrCreateConfigAsync();
        config.PublishMode = mode;
        config.AutoPublishOnApproval = autoPublishOnApproval;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم تحديث إعدادات النشر بنجاح");
    }

    // ── Helper Methods ──

    private async Task<string> BuildCelebratoryCaptionAsync(Service service)
    {
        var providerName = !string.IsNullOrWhiteSpace(service.ProviderProfile?.BusinessName) 
            ? service.ProviderProfile.BusinessName 
            : service.Name;

        var cityName = service.City?.City_Name_AR ?? service.Address ?? "بلدنا";
        var baseSiteUrl = _configuration["BaseUrl"] ?? "https://khadamawy.eis-dev.com";
        var appDownloadUrl = "https://play.google.com/store/apps/details?id=com.nassar84.khadamawy";
        var serviceUrl = $"{baseSiteUrl.TrimEnd('/')}/share/service/{service.Id}";

        var sb = new StringBuilder();
        sb.AppendLine("🎉❤️ عيلة خدماوي بتكبر كل يوم!");
        sb.AppendLine();
        sb.AppendLine("يسعدنا الإعلان عن انضمام خدمة جديدة ومميزة لمجتمعنا المحلي على تطبيق خدماوي:");
        sb.AppendLine();
        sb.AppendLine($"🌟 {service.Name}");
        sb.AppendLine($"👤 مقدم الخدمة: {providerName}");
        sb.AppendLine($"📍 المكان: {cityName}");
        sb.AppendLine();

        // Distinction / welcoming paragraph
        if (service.IsDistinctionEnabled && service.DistinctionReason != "None")
        {
            sb.AppendLine("إحنا بنهتم نعرّف أهل بلدنا بأصحاب الخدمات المميزة، ونساعد شغلك يوصل لكل اللي بيدوروا عليك.");
            sb.AppendLine();
        }

        sb.AppendLine("📱 حمّل تطبيق خدماوي الآن واستكشف الخدمات القريبة منك وتواصل مع أصحابها بضغطة واحدة:");
        sb.AppendLine(appDownloadUrl);
        sb.AppendLine();
        sb.AppendLine("⭐ شوف تفاصيل الخدمة والتقييمات أو تواصل مباشرة:");
        sb.AppendLine(serviceUrl);
        sb.AppendLine();
        sb.AppendLine("تابع صفحتنا الرسمية لمتابعة كل جديد في منطقتك:");
        sb.AppendLine("https://www.facebook.com/khadamawyApp/");
        sb.AppendLine();
        sb.AppendLine("خدماوي.. خدمات بلدنا في مكان واحد ❤️");
        sb.AppendLine("#خدماوي #Khadamawy #مجتمع_خدماوي");

        return sb.ToString();
    }

    private string ConstructPostUrl(string? pageId, string? username, string postId)
    {
        var cleanId = postId;
        if (cleanId.Contains("_"))
        {
            cleanId = cleanId.Split('_').Last();
        }

        var identifier = !string.IsNullOrEmpty(username) ? username : pageId ?? "khadamawyApp";
        return $"https://www.facebook.com/{identifier}/posts/{cleanId}";
    }

    private async Task<FacebookIntegrationConfig> GetOrCreateConfigAsync()
    {
        var config = await _context.FacebookIntegrationConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new FacebookIntegrationConfig
            {
                AppId = _configuration["Authentication:Facebook:AppId"] ?? "1546767603438579",
                PageName = "خدماوى | Khadamawy",
                PageUsername = "KhadamawyApp",
                PageUrl = "https://www.facebook.com/khadamawyApp/",
                PublishMode = "ManualOnly",
                AutoPublishOnApproval = false
            };
            await _context.FacebookIntegrationConfigs.AddAsync(config);
            await _context.SaveChangesAsync();
        }
        return config;
    }

    private string ProtectToken(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken)) return string.Empty;
        return _protector.Protect(rawToken);
    }

    private string UnprotectToken(string protectedToken)
    {
        if (string.IsNullOrEmpty(protectedToken)) return string.Empty;
        try
        {
            return _protector.Unprotect(protectedToken);
        }
        catch
        {
            // Fallback if not encrypted or data protection key changed
            return protectedToken;
        }
    }

    private string MaskString(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 8) return "********";
        return input.Substring(0, 4) + "*******" + input.Substring(input.Length - 4);
    }

    // ── Internal Models for Graph API responses ──
    private class FbTokenResponse
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public long? expires_in { get; set; }
    }

    private class FbAccountsResponse
    {
        public List<FbAccountData>? data { get; set; }
    }

    private class FbAccountData
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? username { get; set; }
        public string? link { get; set; }
        public string? category { get; set; }
        public string access_token { get; set; } = string.Empty;
        public List<string>? tasks { get; set; }
    }

    private class FbPageDetails
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? username { get; set; }
        public string? link { get; set; }
    }

    private class FbPublishResponse
    {
        public string? id { get; set; }
        public string? post_id { get; set; }
    }
}
