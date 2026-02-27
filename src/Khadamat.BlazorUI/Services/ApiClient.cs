using System.Net.Http.Json;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Services.Queries;
using System.Text.Json;
using Khadamat.Application.Features.Services.Commands;

namespace Khadamat.BlazorUI.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly Khadamat.BlazorUI.State.AppState _appState;
    
    public ApiClient(HttpClient http, Khadamat.BlazorUI.State.AppState appState)
    {
        _http = http;
        _appState = appState;
    }

    public async Task<T?> PostAsync<T>(string url, object data)
    {
        var response = await _http.PostAsJsonAsync(url, data);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();
        return default;
    }

    // Settings
    public async Task<ApiResponse<AppSettingsDto>> GetSettingsAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<AppSettingsDto>>("api/v1/settings") 
               ?? ApiResponse<AppSettingsDto>.Fail("Failed to fetch settings");
    }

    public async Task<ApiResponse<bool>> UpdateSettingsAsync(UpdateAppSettingsRequest request)
    {
        var response = await _http.PutAsJsonAsync("api/v1/settings", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Failed to update settings");
    }
 
    // Services
    public async Task<PaginatedResult<ServiceDto>> GetServicesAsync(string? search = null, int? categoryId = null, int? subCategoryId = null, int? governorateId = null, int? cityId = null, string? userId = null, bool? isApproved = true, string? sortBy = "latest", int page = 1, int pageSize = 10)
    {
        var url = $"api/v1/services?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId.HasValue) url += $"&categoryId={categoryId}";
        if (subCategoryId.HasValue) url += $"&subCategoryId={subCategoryId}";
        if (governorateId.HasValue) url += $"&governorateId={governorateId}";
        if (cityId.HasValue) url += $"&cityId={cityId}";
        if (!string.IsNullOrEmpty(userId)) url += $"&userId={Uri.EscapeDataString(userId)}";
        if (isApproved.HasValue) url += $"&isApproved={isApproved}";
        if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}";
        
        try
        {
            return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>(url) ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, pageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching services: {ex.Message}");
            return new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, pageSize);
        }
    }

    public async Task<PaginatedResult<ServiceDto>> GetMyServicesAsync(int page = 1)
    {
        return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>($"api/v1/services/myservices?page={page}") 
               ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, 10);
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ServiceDto>($"api/v1/services/{id}");
    }

    public async Task<int?> CreateServiceAsync(CreateServiceCommand command)
    {
        var response = await _http.PostAsJsonAsync("api/v1/services", command);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetInt32();
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateServiceAsync Failed: {response.StatusCode} - {errorContent}");
        }
        return null;
    }

    public async Task<bool> UpdateServiceAsync(int id, UpdateServiceCommand command)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/services/{id}", command);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/services/{id}");
        return response.IsSuccessStatusCode;
    }

    // Categories
    public async Task<List<MainCategoryDto>> GetMainCategoriesAsync()
    {
        try 
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<MainCategoryDto>>>("api/v1/categories/main");
            return response?.Data ?? new List<MainCategoryDto>();
        }
        catch
        {
            return new List<MainCategoryDto>();
        }
    }

    public async Task<List<CategoryDto>> GetCategoriesByMainIdAsync(int mainId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>($"api/v1/categories/main/{mainId}/categories");
            return response?.Data ?? new List<CategoryDto>();
        }
        catch
        {
            return new List<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CategoryDto>>($"api/v1/categories/categories/{id}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SubCategoryDto>>($"api/v1/categories/subcategories/{id}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int catId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<SubCategoryDto>>>($"api/v1/categories/{catId}/subcategories");
            return response?.Data ?? new List<SubCategoryDto>();
        }
        catch
        {
            return new List<SubCategoryDto>();
        }
    }

    // Category Management
    public async Task<bool> CreateMainCategoryAsync(MainCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories/main", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMainCategoryAsync(int id, MainCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/main/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMainCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/main/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCategoryAsync(CategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateSubCategoryAsync(SubCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories/sub", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSubCategoryAsync(int id, SubCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/sub/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSubCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/sub/{id}");
        return response.IsSuccessStatusCode;
    }

    // Locations
    public async Task<List<GovernorateDto>> GetGovernoratesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GovernorateDto>>>("api/v1/locations/governorates");
        return response?.Data ?? new List<GovernorateDto>();
    }

    public async Task<List<CityDto>> GetCitiesAsync(int governorateId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>($"api/v1/locations/governorates/{governorateId}/cities");
            return response?.Data ?? new List<CityDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching cities: {ex.Message}");
            return new List<CityDto>();
        }
    }

    public async Task<List<CityDto>> GetCitiesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>("api/v1/locations/cities");
        return response?.Data ?? new List<CityDto>();
    }

    public async Task<CityDto?> GetCityByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CityDto>>($"api/v1/locations/cities/{id}");
        return response?.Data;
    }

    public async Task<bool> CreateGovernorateAsync(GovernorateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/locations/governorates", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateGovernorateAsync(int id, GovernorateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/locations/governorates/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGovernorateAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/locations/governorates/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCityAsync(CityDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/locations/cities", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCityAsync(int id, CityDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/locations/cities/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCityAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/locations/cities/{id}");
        return response.IsSuccessStatusCode;
    }

    // Ads
    public async Task<List<EnhancedAdDto>> GetSliderAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("api/v1/ads/slider");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetAllAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("api/v1/ads");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<bool> CreateAdAsync(EnhancedAdDto ad)
    {
        var response = await _http.PostAsJsonAsync("api/v1/ads", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAdAsync(int id, EnhancedAdDto ad)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/ads/{id}", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAdAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/ads/{id}");
        return response.IsSuccessStatusCode;
    }

    // Auth
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/auth/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }
        return null;
    }

    public async Task<ApiResponse<AuthResponse>?> GetProfileAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<AuthResponse>>("api/v1/auth/profile");
    }

    public async Task<dynamic?> GetProviderProfileAsync(string userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<dynamic>($"api/v1/providers/{userId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ApplyProviderAsync(ApplyProviderDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/providers/apply", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProviderProfileAsync(UpdateProviderProfileRequest dto)
    {
        var response = await _http.PutAsJsonAsync("api/v1/providers/profile", dto);
        return response.IsSuccessStatusCode;
    }

    // Admin
    public async Task<AdminStatsDto?> GetAdminStatsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AdminStatsDto>>("api/v1/admin/stats");
        return response?.Data;
    }

    public async Task<List<UserDto>> GetUsersManagementAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UserDto>>>("api/v1/admin/users");
        return response?.Data ?? new List<UserDto>();
    }

    public async Task<List<PendingProviderDto>> GetPendingProvidersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PendingProviderDto>>>("api/v1/admin/providers/pending");
        return response?.Data ?? new List<PendingProviderDto>();
    }

    public async Task<bool> ApproveProviderAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/providers/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<RecentActivityDto>> GetRecentAuditLogsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<RecentActivityDto>>>("api/v1/admin/audit-logs/recent");
        return response?.Data ?? new List<RecentActivityDto>();
    }

    public async Task<bool> RejectProviderAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/providers/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        var response = await _http.PostAsync($"api/v1/admin/users/{id}/toggle-status", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/v1/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveServiceAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/services/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectServiceAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/services/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    // Posts
    public async Task<List<PostDto>> GetProviderPostsAsync(int providerId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PostDto>>>($"api/v1/posts/provider/{providerId}");
        return response?.Data ?? new List<PostDto>();
    }

    public async Task<bool> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/posts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/posts/{id}");
        return response.IsSuccessStatusCode;
    }

    // Reviews (My Ratings/Comments)
    public async Task<List<MyReviewDto>> GetMyReviewsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyReviewDto>>>("api/v1/reviews/my");
        return response?.Data ?? new List<MyReviewDto>();
    }

    public async Task<bool> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/reviews", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddReviewAsync(int serviceId, int rating, string comment)
    {
        return await CreateReviewAsync(new CreateReviewRequest { ServiceId = serviceId, Rating = rating, Comment = comment });
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/reviews/{id}");
        return response.IsSuccessStatusCode;
    }

    // Favorites
    public async Task<List<ServiceDto>> GetMyFavoritesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ServiceDto>>>("api/v1/favorites");
        return response?.Data ?? new List<ServiceDto>();
    }

    public async Task<bool> ToggleFavoriteAsync(int serviceId)
    {
        var response = await _http.PostAsync($"api/v1/favorites/toggle/{serviceId}", null);
        return response.IsSuccessStatusCode;
    }

    // Comments
    public async Task<List<MyCommentDto>> GetMyCommentsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyCommentDto>>>("api/v1/comments/my");
        return response?.Data ?? new List<MyCommentDto>();
    }

    public async Task<bool> CreateCommentAsync(CreateCommentRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/comments", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCommentAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/comments/{id}");
        return response.IsSuccessStatusCode;
    }
    // Notifications
    public async Task<List<NotificationDto>> GetMyNotificationsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>("api/v1/notifications");
        return response?.Data ?? new List<NotificationDto>();
    }

    public async Task<bool> MarkNotificationAsReadAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/notifications/{id}/read", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        var response = await _http.PostAsync("api/v1/notifications/read-all", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendAdminNotificationAsync(string? userId, string title, string message, string? link = null, string? role = null, int? govId = null, int? cityId = null, int? catId = null)
    {
        var request = new { 
            UserId = userId, 
            Title = title, 
            Message = message, 
            RelatedLink = link,
            TargetRole = role,
            GovernorateId = govId,
            CityId = cityId,
            MainCategoryId = catId
        };
        var response = await _http.PostAsJsonAsync("api/v1/notifications/send", request);
        return response.IsSuccessStatusCode;
    }

    // Messages
    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ConversationDto>>>("api/v1/messages");
        return response?.Data ?? new List<ConversationDto>();
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string userId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MessageDto>>>($"api/v1/messages/{userId}");
        return response?.Data ?? new List<MessageDto>();
    }

    public async Task<bool> SendMessageAsync(string receiverId, string content)
    {
        var request = new { ReceiverId = receiverId, Content = content };
        var response = await _http.PostAsJsonAsync("api/v1/messages", request);
        return response.IsSuccessStatusCode;
    }

    // Subscriptions
    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<SubscriptionPlanDto>>("api/v1/subscriptions/plans") 
                   ?? new List<SubscriptionPlanDto>();
        }
        catch
        {
            return new List<SubscriptionPlanDto>();
        }
    }

    public async Task<ProviderSubscriptionDto?> GetMySubscriptionAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/v1/subscriptions/my-subscription");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProviderSubscriptionDto>();
            }
        }
        catch { }
        return null;
    }

    public async Task<ApiResponse<bool>> SubscribeAsync(int planId)
    {
        var response = await _http.PostAsJsonAsync("api/v1/subscriptions/subscribe", new SubscribeRequest { PlanId = planId });
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("فشل الاشتراك في الباقة");
    }

    // Service Requests
    public async Task<List<ServiceRequestDto>> GetMyRequestsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ServiceRequestDto>>("api/v1/requests/my-requests");
            return response ?? new List<ServiceRequestDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching requests: {ex.Message}");
            return new List<ServiceRequestDto>();
        }
    }

    public async Task<ApiResponse<int>> CreateRequestAsync(CreateServiceRequestDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/v1/requests", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<int>>() 
                       ?? ApiResponse<int>.Fail("فشل في إنشاء الطلب");
            }
            return ApiResponse<int>.Fail("فشل في إنشاء الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating request: {ex.Message}");
            return ApiResponse<int>.Fail(ex.Message);
        }
    }

    public async Task<List<ServiceRequestDto>> GetProviderRequestsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ServiceRequestDto>>("api/v1/requests/provider-requests");
            return response ?? new List<ServiceRequestDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching provider requests: {ex.Message}");
            return new List<ServiceRequestDto>();
        }
    }

    public async Task<ApiResponse<bool>> UpdateRequestStatusAsync(int requestId, Khadamat.Domain.Enums.RequestStatus status, string? notes = null)
    {
        try
        {
            var command = new { Status = status, ProviderNotes = notes };
            var response = await _http.PutAsJsonAsync($"api/v1/requests/{requestId}/status", command);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                       ?? ApiResponse<bool>.Fail("فشل في تحديث حالة الطلب");
            }
            return ApiResponse<bool>.Fail("فشل في تحديث حالة الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating request status: {ex.Message}");
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelRequestAsync(int requestId)
    {
        try
        {
            var response = await _http.PutAsync($"api/v1/requests/my-requests/{requestId}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                       ?? ApiResponse<bool>.Fail("فشل في إلغاء الطلب");
            }
            return ApiResponse<bool>.Fail("فشل في إلغاء الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling request: {ex.Message}");
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }

    // Marketplace
    public async Task<List<MarketplaceItemDto>> GetLatestMarketplaceItemsAsync(int count = 10)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"api/Marketplace/latest?count={count}") ?? new List<MarketplaceItemDto>();
        }
        catch { return new List<MarketplaceItemDto>(); }
    }

    public async Task<List<MarketplaceItemDto>> GetFeaturedMarketplaceItemsAsync(int count = 6)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"api/Marketplace/featured?count={count}") ?? new List<MarketplaceItemDto>();
        }
        catch { return new List<MarketplaceItemDto>(); }
    }

    public async Task<List<MarketplaceItemDto>> SearchMarketplaceItemsAsync(string? q = null, int? categoryId = null, int? subCategoryId = null, int? governorateId = null, int? cityId = null, string? condition = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 12)
    {
        try
        {
            var url = $"api/Marketplace/search?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(q)) url += $"&q={Uri.EscapeDataString(q)}";
            if (categoryId.HasValue && categoryId > 0) url += $"&categoryId={categoryId}";
            if (subCategoryId.HasValue && subCategoryId > 0) url += $"&subCategoryId={subCategoryId}";
            if (governorateId.HasValue && governorateId > 0) url += $"&governorateId={governorateId}";
            if (cityId.HasValue && cityId > 0) url += $"&cityId={cityId}";
            if (!string.IsNullOrEmpty(condition)) url += $"&condition={condition}";
            if (minPrice.HasValue && minPrice > 0) url += $"&minPrice={minPrice}";
            if (maxPrice.HasValue && maxPrice > 0) url += $"&maxPrice={maxPrice}";

            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>(url) ?? new List<MarketplaceItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching marketplace: {ex.Message}");
            return new List<MarketplaceItemDto>();
        }
    }

    public async Task<MarketplaceItemDto?> GetMarketplaceItemByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<MarketplaceItemDto>($"api/Marketplace/{id}");
        }
        catch { return null; }
    }

    public async Task<MarketplaceItemDto?> CreateMarketplaceItemAsync(CreateMarketplaceItemRequest request)
    {
        try 
        {
            var response = await _http.PostAsJsonAsync("api/Marketplace", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<MarketplaceItemDto>();
            
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateMarketplaceItem Error: {response.StatusCode} - {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateMarketplaceItem Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateMarketplaceItemAsync(int id, CreateMarketplaceItemRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/Marketplace/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMarketplaceItemAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/Marketplace/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkMarketplaceItemAsSoldAsync(int id)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/sold", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangeMarketplaceItemStatusAsync(int id, string status)
    {
        try
        {
            var request = new { Status = status };
            var response = await _http.PostAsJsonAsync($"api/Marketplace/{id}/change-status", request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> LockMarketplaceItemAsync(int id)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/lock", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleMarketplaceFavoriteAsync(int id)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/favorite", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsMarketplaceFavoriteAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<bool>($"api/Marketplace/{id}/is-favorite");
        }
        catch { return false; }
    }

    // Admin Marketplace
    public async Task<List<MarketplaceItemDto>> GetAllMarketplaceItemsAdminAsync(int pageSize = 300)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"api/Marketplace/admin/items?pageSize={pageSize}") ?? new List<MarketplaceItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllMarketplaceItemsAdmin error: {ex.Message}");
            return new List<MarketplaceItemDto>();
        }
    }

    public async Task<MarketplaceSettingsDto?> GetMarketplaceSettingsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<MarketplaceSettingsDto>("api/Marketplace/settings");
        }
        catch { return null; }
    }

    public async Task<bool> ApproveMarketplaceItemAsync(int id, string? notes = null)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/approve?notes={Uri.EscapeDataString(notes ?? "")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveMarketplaceItemWithDatesAsync(int id, DateTime startDate, DateTime endDate, string? notes = null)
    {
        try
        {
            var url = $"api/Marketplace/{id}/approve?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(notes))
                url += $"&notes={Uri.EscapeDataString(notes)}";
            var response = await _http.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RejectMarketplaceItemAsync(int id, string? notes = null)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/reject?notes={Uri.EscapeDataString(notes ?? "")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetMarketplaceItemFeaturedAsync(int id, int days = 7)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/set-featured?days={days}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetMarketplaceItemPromotedAsync(int id, int days = 7)
    {
        var response = await _http.PostAsync($"api/Marketplace/{id}/set-promoted?days={days}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<MarketplaceItemDto>> GetMyMarketplaceItemsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>("api/Marketplace/my-items") ?? new List<MarketplaceItemDto>();
        }
        catch { return new List<MarketplaceItemDto>(); }
    }

    public async Task<List<MarketplaceCategoryDto>> GetMarketplaceCategoriesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceCategoryDto>>("api/Marketplace/categories") ?? new List<MarketplaceCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching marketplace categories: {ex.Message}");
            return new List<MarketplaceCategoryDto>();
        }
    }

    public async Task<List<MarketplaceSubCategoryDto>> GetMarketplaceSubCategoriesAsync(int categoryId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceSubCategoryDto>>($"api/Marketplace/categories/{categoryId}/subcategories") ?? new List<MarketplaceSubCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching marketplace subcategories: {ex.Message}");
            return new List<MarketplaceSubCategoryDto>();
        }
    }
}


// DTOs for Client usage
public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class CreateReviewRequest
{
    public int ServiceId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class MyReviewDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class MyCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string PostContentSnippet { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
public class AuthResponseDto { public string Token { get; set; } = ""; public string RefreshToken { get; set; } = ""; public string FullName { get; set; } = ""; }
