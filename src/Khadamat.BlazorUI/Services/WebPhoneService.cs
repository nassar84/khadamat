using Khadamat.Shared.Interfaces;
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services;

public class WebPhoneService : IPhoneService
{
    private readonly IJSRuntime _js;

    public WebPhoneService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task MakePhoneCallAsync(string phoneNumber)
    {
        await _js.InvokeVoidAsync("location.assign", $"tel:{phoneNumber}");
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        await _js.InvokeVoidAsync("location.assign", $"sms:{phoneNumber}?body={Uri.EscapeDataString(message)}");
    }

    public async Task OpenWhatsAppChatAsync(string phoneNumber, string? message = null)
    {
        var url = string.IsNullOrEmpty(message) 
            ? $"https://wa.me/{phoneNumber}"
            : $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";
        await _js.InvokeVoidAsync("location.assign", url);
    }

    public bool CanMakePhoneCalls => true;

    public Task<bool> IsWhatsAppInstalledAsync() => Task.FromResult(true);
}
