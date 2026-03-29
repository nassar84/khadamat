
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services;

public class SoundService
{
    private readonly IJSRuntime _js;
    private readonly State.AppState _state;

    public SoundService(IJSRuntime js, State.AppState state)
    {
        _js = js;
        _state = state;
    }

    public async Task PlayStartupAsync()
    {
        string sound = string.IsNullOrEmpty(_state.OpenAppSound) ? "bic_ring1.mp3" : _state.OpenAppSound;
        await PlaySoundAsync(sound);
    }

    public async Task PlayNotificationAsync()
    {
        await PlaySoundAsync(_state.NotificationReceivedSound ?? "notification.mp3");
    }

    public async Task PlayMessageAsync()
    {
        await PlaySoundAsync(_state.MessageReceivedSound ?? "message.mp3");
    }

    public async Task PlayFindServiceAsync()
    {
        await PlaySoundAsync(_state.FindServiceSound);
    }

    public async Task PlayOpenDetailsAsync()
    {
        await PlaySoundAsync(_state.OpenDetailsSound);
    }

    private async Task PlaySoundAsync(string? soundFile)
    {
        if (string.IsNullOrEmpty(soundFile)) return;
        try 
        { 
            // Call the JS function for robust playback and fallback
            await _js.InvokeVoidAsync("KhadamatSounds.playStoredSound", soundFile); 
        } 
        catch { }
    }
}
