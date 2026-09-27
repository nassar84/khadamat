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
        try
        {
            var result = await _js.InvokeAsync<ShareResult>("nativeShare", title, text, null);
            if (result is { Success: true }) return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareText error: {ex.Message}");
        }

        try
        {
            await _js.InvokeVoidAsync("navigator.share", new { title = title, text = text });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareText fallback error: {ex.Message}");
        }
    }

    public async Task ShareLinkAsync(string url, string title = "مشاركة رابط")
    {
        try
        {
            var text = string.IsNullOrEmpty(title) ? url : $"{title}\n{url}";
            var result = await _js.InvokeAsync<ShareResult>("nativeShare", title, text, url);
            if (result is { Success: true }) return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareLink error: {ex.Message}");
        }

        try
        {
            await _js.InvokeVoidAsync("navigator.share", new { title = title, url = url });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareLink fallback error: {ex.Message}");
        }
    }

    public async Task ShareFileAsync(string filePath, string title = "مشاركة ملف")
    {
        // Web share API doesn't easily share file paths, usually needs File object
    }

    public async Task ShareFilesAsync(List<string> filePaths, string title = "مشاركة ملفات")
    {
        // No-op for web fallback
    }

    public async Task ShareImageWithTextAsync(string imageUrl, string text, string title = "مشاركة خدمة")
    {
        try
        {
            var result = await _js.InvokeAsync<ShareResult>("nativeShare", title, text, null, imageUrl);
            if (result is { Success: true }) return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareImageWithText error: {ex.Message}");
        }

        try
        {
            await _js.InvokeVoidAsync("navigator.share", new { title = title, text = text, url = imageUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShareImageWithText fallback error: {ex.Message}");
        }
    }

    private class ShareResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}
