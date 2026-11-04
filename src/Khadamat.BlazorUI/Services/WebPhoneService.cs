using Khadamat.Domain.Common;
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
        var telUrl = PhoneNumberHelper.GetTelUrl(phoneNumber);
        if (!string.IsNullOrEmpty(telUrl) && telUrl != "#")
        {
            await _js.InvokeVoidAsync("location.assign", telUrl);
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        var clean = PhoneNumberHelper.NormalizeForCall(phoneNumber);
        if (!string.IsNullOrEmpty(clean))
        {
            await _js.InvokeVoidAsync("location.assign", $"sms:{clean}?body={Uri.EscapeDataString(message)}");
        }
    }

    public async Task OpenWhatsAppChatAsync(string phoneNumber, string? message = null)
    {
        var url = PhoneNumberHelper.GetWhatsAppUrl(phoneNumber, message);
        if (!string.IsNullOrEmpty(url) && url != "#")
        {
            await _js.InvokeVoidAsync("location.assign", url);
        }
    }

    public bool CanMakePhoneCalls => true;

    public Task<bool> IsWhatsAppInstalledAsync() => Task.FromResult(true);
}
