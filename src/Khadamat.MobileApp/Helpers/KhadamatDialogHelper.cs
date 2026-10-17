using CommunityToolkit.Maui.Views;
using Khadamat.MobileApp.Views.Popups;
using Microsoft.Maui.Controls;

namespace Khadamat.MobileApp.Helpers;

public static class KhadamatDialogHelper
{
    private static Page? GetCurrentPage(Page? page = null)
    {
        return page ?? Microsoft.Maui.Controls.Application.Current?.MainPage;
    }

    public static async Task ShowAlertAsync(
        string title,
        string message,
        string buttonText = "حسناً",
        MobileDialogType type = MobileDialogType.Info,
        Page? page = null)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var targetPage = GetCurrentPage(page);
            if (targetPage == null) return;

            try
            {
                var popup = new KhadamatAlertPopup(title, message, isConfirm: false, confirmText: buttonText, type: type);
                await targetPage.ShowPopupAsync(popup);
            }
            catch
            {
                // Fallback to native DisplayAlert if popup cannot be opened
                await targetPage.DisplayAlert(title, message, buttonText);
            }
        });
    }

    public static async Task<bool> ShowConfirmAsync(
        string title,
        string message,
        string confirmText = "موافق",
        string cancelText = "إلغاء",
        MobileDialogType type = MobileDialogType.Warning,
        Page? page = null)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var targetPage = GetCurrentPage(page);
            if (targetPage == null) return false;

            try
            {
                var popup = new KhadamatAlertPopup(title, message, isConfirm: true, confirmText: confirmText, cancelText: cancelText, type: type);
                var result = await targetPage.ShowPopupAsync(popup);
                return result is bool b && b;
            }
            catch
            {
                return await targetPage.DisplayAlert(title, message, confirmText, cancelText);
            }
        });
    }

    public static Task ShowSuccessAsync(string title, string message, Page? page = null)
        => ShowAlertAsync(title, message, "حسناً", MobileDialogType.Success, page);

    public static Task ShowErrorAsync(string title, string message, Page? page = null)
        => ShowAlertAsync(title, message, "حسناً", MobileDialogType.Error, page);

    public static Task ShowWarningAsync(string title, string message, Page? page = null)
        => ShowAlertAsync(title, message, "حسناً", MobileDialogType.Warning, page);
}
