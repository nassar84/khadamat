using Khadamat.Shared.Interfaces;
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services;

public class WebShareService : IShareService
{
    private readonly IJSRuntime _js;

    public WebShareService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task ShareTextAsync(string text, string title = "مشاركة")
    {
        await _js.InvokeVoidAsync("navigator.share", new { title = title, text = text });
    }

    public async Task ShareLinkAsync(string url, string title = "مشاركة رابط")
    {
        await _js.InvokeVoidAsync("navigator.share", new { title = title, url = url });
    }

    public async Task ShareFileAsync(string filePath, string title = "مشاركة ملف")
    {
        // Web share API doesn't easily share file paths, usually needs File object
        // For now, no-op or fallback
    }

    public async Task ShareFilesAsync(List<string> filePaths, string title = "مشاركة ملفات")
    {
        // No-op for web fallback
    }
}
