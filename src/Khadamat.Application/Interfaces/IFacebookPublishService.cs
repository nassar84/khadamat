using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface IFacebookPublishService
{
    Task<FacebookIntegrationStatusDto> GetIntegrationStatusAsync();
    Task<FacebookOAuthInitResponseDto> InitiateAdminAuthorizationAsync(string returnUrl);
    Task<ApiResponse<List<FacebookManagedPageDto>>> ProcessOAuthCallbackAsync(string code, string state);
    Task<ApiResponse<bool>> ConnectPageAsync(FacebookConnectPageRequestDto request, string adminUserId, string adminUserName);
    Task<ApiResponse<bool>> DisconnectPageAsync(string adminUserId);
    Task<FacebookConnectionTestResultDto> ValidatePageConnectionAsync();
    Task<FacebookConnectionTestResultDto> ValidatePublishingPermissionsAsync();
    
    Task<ApiResponse<FacebookPostResultDto>> PublishTextPostAsync(string message, string? adminUserId = null, string? adminUserName = null);
    Task<ApiResponse<FacebookPostResultDto>> PublishPhotoPostAsync(string caption, string photoUrl, string? adminUserId = null, string? adminUserName = null);
    Task<ApiResponse<FacebookPostResultDto>> PublishServiceCardAsync(int serviceId, string? customCaption = null, string? adminUserId = null, string? adminUserName = null);
    Task<ApiResponse<string>> PrepareServiceCardPostTextAsync(int serviceId);

    Task<List<FacebookPublishLogDto>> GetPublishLogsAsync(int? serviceId = null, string? status = null, int take = 50);
    Task<ApiResponse<bool>> RetryPublishAsync(int logId, string adminUserId);
    Task<ApiResponse<bool>> CancelPublishAsync(int logId, string adminUserId);
    Task<ApiResponse<bool>> UpdatePublishModeAsync(string mode, bool autoPublishOnApproval);
}
