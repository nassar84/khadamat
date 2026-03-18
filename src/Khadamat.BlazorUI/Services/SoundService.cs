
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services;

public class SoundService
{
    private readonly IJSRuntime _js;

    public SoundService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task PlayStartupAsync()
    {
        try { await _js.InvokeVoidAsync("KhadamatSounds.playStartup"); } catch { }
    }

    public async Task PlayNotificationAsync()
    {
        try { await _js.InvokeVoidAsync("KhadamatSounds.playNotification"); } catch { }
    }

    public async Task PlayMessageAsync()
    {
        try { await _js.InvokeVoidAsync("KhadamatSounds.playMessage"); } catch { }
    }
}
