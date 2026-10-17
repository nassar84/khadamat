using Khadamat.BlazorUI.Models;

namespace Khadamat.BlazorUI.Services;

public interface IFeedbackService
{
    event Action<ToastItem>? OnToastAdded;
    event Action<string>? OnToastRemoved;
    event Func<ConfirmDialogOptions, Task<bool>>? OnConfirmRequested;

    void ShowToast(string title, string message = "", FeedbackType type = FeedbackType.Info, int durationMs = 4500);
    void ShowSuccess(string title, string message = "", int durationMs = 4500);
    void ShowError(string title, string message = "", int durationMs = 5500);
    void ShowWarning(string title, string message = "", int durationMs = 5000);
    void ShowInfo(string title, string message = "", int durationMs = 4500);

    void RemoveToast(string id);

    Task<bool> ConfirmAsync(string message, string title = "تأكيد الإجراء", FeedbackType type = FeedbackType.Warning, string confirmText = "تأكيد", string cancelText = "إلغاء");
    Task<bool> ConfirmDangerAsync(string message, string title = "تأكيد الحذف", string confirmText = "نعم، حذف", string cancelText = "تراجع");
}

public class KhadamatFeedbackService : IFeedbackService
{
    public event Action<ToastItem>? OnToastAdded;
    public event Action<string>? OnToastRemoved;
    public event Func<ConfirmDialogOptions, Task<bool>>? OnConfirmRequested;

    public void ShowToast(string title, string message = "", FeedbackType type = FeedbackType.Info, int durationMs = 4500)
    {
        var item = new ToastItem
        {
            Title = title,
            Message = message,
            Type = type,
            DurationMs = durationMs
        };
        OnToastAdded?.Invoke(item);
    }

    public void ShowSuccess(string title, string message = "", int durationMs = 4500)
        => ShowToast(title, message, FeedbackType.Success, durationMs);

    public void ShowError(string title, string message = "", int durationMs = 5500)
        => ShowToast(title, message, FeedbackType.Error, durationMs);

    public void ShowWarning(string title, string message = "", int durationMs = 5000)
        => ShowToast(title, message, FeedbackType.Warning, durationMs);

    public void ShowInfo(string title, string message = "", int durationMs = 4500)
        => ShowToast(title, message, FeedbackType.Info, durationMs);

    public void RemoveToast(string id)
    {
        OnToastRemoved?.Invoke(id);
    }

    public async Task<bool> ConfirmAsync(string message, string title = "تأكيد الإجراء", FeedbackType type = FeedbackType.Warning, string confirmText = "تأكيد", string cancelText = "إلغاء")
    {
        if (OnConfirmRequested != null)
        {
            return await OnConfirmRequested.Invoke(new ConfirmDialogOptions
            {
                Title = title,
                Message = message,
                Type = type,
                ConfirmText = confirmText,
                CancelText = cancelText
            });
        }
        // Fallback: return true if no UI handler attached
        return true;
    }

    public Task<bool> ConfirmDangerAsync(string message, string title = "تأكيد الحذف", string confirmText = "نعم، حذف", string cancelText = "تراجع")
    {
        return ConfirmAsync(message, title, FeedbackType.Error, confirmText, cancelText);
    }
}
