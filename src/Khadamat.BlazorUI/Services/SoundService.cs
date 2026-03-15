
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
        await _js.InvokeVoidAsync("KhadamatSounds.playStartup");
    }

    public async Task PlayNotificationAsync()
    {
        await _js.InvokeVoidAsync("KhadamatSounds.playNotification");
    }

    public async Task PlayMessageAsync()
    {
        await _js.InvokeVoidAsync("KhadamatSounds.playMessage");
    }
}
