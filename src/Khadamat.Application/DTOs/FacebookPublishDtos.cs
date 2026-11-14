using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class FacebookIntegrationStatusDto
{
    public bool IsConnected { get; set; }
    public string MaskedAppId { get; set; } = string.Empty;
    public string PageName { get; set; } = "خدماوى | Khadamawy";
    public string PageUsername { get; set; } = "KhadamawyApp";
    public string PageUrl { get; set; } = "https://www.facebook.com/khadamawyApp/";
    public string? PageId { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public string? ConnectedByAdminName { get; set; }
    public string? PermissionsGranted { get; set; }
    public bool HasManagePostsPermission { get; set; }
    public string PublishMode { get; set; } = "ManualOnly";
    public bool AutoPublishOnApproval { get; set; }
    public DateTime? LastConnectionCheck { get; set; }
    public bool LastConnectionSuccess { get; set; }
    public string? LastConnectionError { get; set; }
    public int TotalPublishedPosts { get; set; }
    public int PendingReviewCount { get; set; }
    public int FailedCount { get; set; }
}

public class FacebookManagedPageDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Category { get; set; }
    public string? Link { get; set; }
    public List<string> Tasks { get; set; } = new();
    public bool CanCreateContent { get; set; }
    public bool IsOfficialKhadamawyPage { get; set; }
    public string? PageAccessToken { get; set; }
}

public class FacebookConnectPageRequestDto
{
    public string PageId { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string? PageUsername { get; set; }
    public string? PageUrl { get; set; }
    public string PageAccessToken { get; set; } = string.Empty;
    public string? Tasks { get; set; }
    public string? Permissions { get; set; }
}

public class FacebookPublishTextRequestDto
{
    public string Message { get; set; } = string.Empty;
}

public class FacebookPublishPhotoRequestDto
{
    public string Caption { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}

public class FacebookPublishServiceCardRequestDto
{
    public int ServiceId { get; set; }
    public string? CustomCaption { get; set; }
}

public class FacebookPostResultDto
{
    public string? PostId { get; set; }
    public string? PostUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class FacebookPublishLogDto
{
    public int Id { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string PostType { get; set; } = "ServiceCard";
    public string? FacebookPostId { get; set; }
    public string? PostUrl { get; set; }
    public string PostText { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "PendingReview";
    public DateTime? AttemptedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AdminUserName { get; set; }
    public string? AdminNotes { get; set; }
}

public class FacebookConnectionTestResultDto
{
    public bool Success { get; set; }
    public string? PageId { get; set; }
    public string? PageName { get; set; }
    public bool CanPublish { get; set; }
    public string? Permissions { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}

public class FacebookOAuthInitResponseDto
{
    public string OAuthUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class FacebookUpdateModeDto
{
    public string PublishMode { get; set; } = "ManualOnly";
    public bool AutoPublishOnApproval { get; set; } = false;
}
