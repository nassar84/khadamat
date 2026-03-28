
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
        await PlaySoundAsync(_state.OpenAppSound ?? "startup.mp3");
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
            var audioUrl = $"/audio/{soundFile}";
            if (soundFile.StartsWith("http")) audioUrl = soundFile;
            
            await _js.InvokeVoidAsync("eval", $"new Audio('{audioUrl}').play()"); 
        } 
        catch { }
    }
}
