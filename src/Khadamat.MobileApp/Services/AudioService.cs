using Plugin.Maui.Audio;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class AudioService : IAudioService
{
    private readonly IAudioManager _audioManager;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, byte[]> _cache = new();
    private string _baseUrl = "";

    public AudioService(IAudioManager audioManager, IConfiguration configuration)
    {
        _audioManager = audioManager;
        _configuration = configuration;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _baseUrl = _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
    }

    public async Task InitializeAsync() => await Task.CompletedTask;

    public async Task PlaySoundAsync(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName)) return;

        try
        {
            byte[]? audioData;
            
            // Check cache
            if (!_cache.TryGetValue(soundFileName, out audioData))
            {
                string url = soundFileName.StartsWith("http") 
                    ? soundFileName 
                    : $"{_baseUrl}/audio/{soundFileName}";

                audioData = await _httpClient.GetByteArrayAsync(url);
                _cache.TryAdd(soundFileName, audioData);
            }

            if (audioData != null)
            {
                var stream = new MemoryStream(audioData);
                var player = _audioManager.CreatePlayer(stream);
                player.Play();
                
                // Note: Player should be disposed usually, but for short sounds we might let it be
                // or handle disposal after PlaybackEnded if supported.
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: AudioService Error playing {soundFileName}: {ex.Message}");
        }
    }
}
