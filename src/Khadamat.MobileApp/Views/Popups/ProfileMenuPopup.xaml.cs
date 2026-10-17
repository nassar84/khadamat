using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace Khadamat.MobileApp.Views.Popups;

public partial class ProfileMenuPopup : Popup
{
    public ProfileMenuPopup(string userName, string userRole, string? userImage, VisualElement? anchor = null)
    {
        InitializeComponent();

        if (anchor != null)
        {
            Anchor = anchor as View;
        }

        UserNameLabel.Text = string.IsNullOrWhiteSpace(userName) ? "مستخدم" : userName;
        UserRoleLabel.Text = string.IsNullOrWhiteSpace(userRole) ? "حساب نشط" : userRole;

        if (!string.IsNullOrEmpty(userImage) && 
            userImage != "profile_icon.png" && 
            userImage != "app_logo.png" && 
            !userImage.ToLower().Contains("default"))
        {
            if (userImage.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                userImage.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                UserAvatarImage.Source = new UriImageSource { Uri = new Uri(userImage), CachingEnabled = false };
            }
            else
            {
                var fullUrl = ViewModels.ShellViewModel.ResolveFullImageUrl(userImage);
                UserAvatarImage.Source = new UriImageSource { Uri = new Uri(fullUrl), CachingEnabled = false };
            }
        }
        else
        {
            UserAvatarImage.Source = "app_logo.png";
        }
    }

    private async void OnProfileClicked(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement v)
        {
            await v.ScaleTo(0.96, 70);
            await v.ScaleTo(1.0, 70);
        }
        Close("profile");
    }

    private async void OnLogoutClicked(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement v)
        {
            await v.ScaleTo(0.96, 70);
            await v.ScaleTo(1.0, 70);
        }
        Close("logout");
    }
}
