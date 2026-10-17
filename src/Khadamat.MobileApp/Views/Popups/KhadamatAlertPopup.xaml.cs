using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Khadamat.MobileApp.Views.Popups;

public enum MobileDialogType
{
    Success,
    Error,
    Warning,
    Info
}

public partial class KhadamatAlertPopup : Popup
{
    public KhadamatAlertPopup(
        string title,
        string message,
        bool isConfirm = false,
        string confirmText = "حسناً",
        string cancelText = "إلغاء",
        MobileDialogType type = MobileDialogType.Info)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        ConfirmBtnLabel.Text = confirmText;
        CancelBtnLabel.Text = cancelText;

        if (!isConfirm)
        {
            CancelBorder.IsVisible = false;
            CancelColumn.Width = 0;
        }

        ApplyStyling(type);
    }

    private void ApplyStyling(MobileDialogType type)
    {
        switch (type)
        {
            case MobileDialogType.Success:
                IconBadgeBorder.BackgroundColor = Color.FromArgb("#ecfdf5");
                IconLabel.Text = "✅";
                ConfirmBorder.BackgroundColor = Color.FromArgb("#10b981");
                break;

            case MobileDialogType.Error:
                IconBadgeBorder.BackgroundColor = Color.FromArgb("#fef2f2");
                IconLabel.Text = "❌";
                ConfirmBorder.BackgroundColor = Color.FromArgb("#ef4444");
                break;

            case MobileDialogType.Warning:
                IconBadgeBorder.BackgroundColor = Color.FromArgb("#fffbeb");
                IconLabel.Text = "⚠️";
                ConfirmBorder.BackgroundColor = Color.FromArgb("#f59e0b");
                break;

            case MobileDialogType.Info:
            default:
                IconBadgeBorder.BackgroundColor = Color.FromArgb("#eff6ff");
                IconLabel.Text = "ℹ️";
                ConfirmBorder.BackgroundColor = Color.FromArgb("#2563eb");
                break;
        }
    }

    private void OnConfirmClicked(object? sender, EventArgs e)
    {
        Close(true);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
