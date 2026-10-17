namespace Khadamat.BlazorUI.Models;

public enum FeedbackType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public FeedbackType Type { get; set; } = FeedbackType.Info;
    public int DurationMs { get; set; } = 4500;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string IconClass => Type switch
    {
        FeedbackType.Success => "fa-solid fa-circle-check",
        FeedbackType.Error => "fa-solid fa-circle-xmark",
        FeedbackType.Warning => "fa-solid fa-triangle-exclamation",
        FeedbackType.Info => "fa-solid fa-circle-info",
        _ => "fa-solid fa-bell"
    };

    public string BadgeText => Type switch
    {
        FeedbackType.Success => "نجاح",
        FeedbackType.Error => "تنبيه خطأ",
        FeedbackType.Warning => "تحذير",
        FeedbackType.Info => "معلومة",
        _ => "إشعار"
    };

    public string CssClass => Type switch
    {
        FeedbackType.Success => "toast-success",
        FeedbackType.Error => "toast-error",
        FeedbackType.Warning => "toast-warning",
        FeedbackType.Info => "toast-info",
        _ => "toast-info"
    };
}

public class ConfirmDialogOptions
{
    public string Title { get; set; } = "تأكيد الإجراء";
    public string Message { get; set; } = string.Empty;
    public string ConfirmText { get; set; } = "تأكيد";
    public string CancelText { get; set; } = "إلغاء";
    public FeedbackType Type { get; set; } = FeedbackType.Warning;
    public string? ConfirmButtonClass { get; set; }

    public string IconClass => Type switch
    {
        FeedbackType.Success => "fa-solid fa-check",
        FeedbackType.Error => "fa-solid fa-trash-can",
        FeedbackType.Warning => "fa-solid fa-triangle-exclamation",
        FeedbackType.Info => "fa-solid fa-circle-question",
        _ => "fa-solid fa-circle-question"
    };
}
