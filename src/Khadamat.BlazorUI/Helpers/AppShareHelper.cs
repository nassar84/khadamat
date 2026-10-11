using System.Threading.Tasks;
using Khadamat.BlazorUI.State;
using Khadamat.Shared.Interfaces;
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Helpers;

public static class AppShareHelper
{
    public static string GetDownloadUrl(AppState state, string baseUri)
    {
        if (!string.IsNullOrEmpty(state.AppShareUrl)) return state.AppShareUrl;
        if (!string.IsNullOrEmpty(state.GooglePlayUrl)) return state.GooglePlayUrl;
        return $"{baseUri.TrimEnd('/')}/downloads/khadamat.apk";
    }

    public static string GetInvitationText(AppState state, string baseUri)
    {
        var appName = !string.IsNullOrEmpty(state.AppNameAr) ? state.AppNameAr : (!string.IsNullOrEmpty(state.AppName) ? state.AppName : "خدماوي");
        var downloadUrl = GetDownloadUrl(state, baseUri);
        return $"📲 منصة وتطبيق {appName} — كل خدماتك في مكان واحد!\n" +
               "تواصل مباشرة مع أفضل الحرفيين والمهنيين ومقدمي الخدمات بكل سهولة وأمان.\n\n" +
               $"📱 رابط تحميل التطبيق:\n{downloadUrl}";
    }

    public static string GetLogoUrl(AppState state)
    {
        return state.GetAbsoluteUrl(string.IsNullOrEmpty(state.AppLogo) ? "images/logo.png" : state.AppLogo);
    }

    public static async Task ShareAppAsync(IShareService? shareService, AppState state, string baseUri, IJSRuntime? js = null)
    {
        var appName = !string.IsNullOrEmpty(state.AppNameAr) ? state.AppNameAr : (!string.IsNullOrEmpty(state.AppName) ? state.AppName : "خدماوي");
        var logoUrl = GetLogoUrl(state);
        var text = GetInvitationText(state, baseUri);
        var title = $"مشاركة تطبيق {appName}";

        if (shareService != null)
        {
            await shareService.ShareImageWithTextAsync(logoUrl, text, title);
        }
        else if (js != null)
        {
            var downloadUrl = GetDownloadUrl(state, baseUri);
            await js.InvokeVoidAsync("shareApp", title, text, downloadUrl, logoUrl);
        }
    }
}
