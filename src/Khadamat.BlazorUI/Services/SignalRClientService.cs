using Microsoft.AspNetCore.SignalR.Client;
using Khadamat.Application.DTOs;
using Khadamat.BlazorUI.State;
using Microsoft.AspNetCore.Components;

namespace Khadamat.BlazorUI.Services;

public class SignalRClientService
{
    private HubConnection? _notificationHub;
    private HubConnection? _chatHub;
    private readonly AppState _appState;
    private readonly Khadamat.Shared.Interfaces.ISecureStorageService _secureStorage;
    private readonly NavigationManager _navigation;
    private readonly string _apiBaseUrl = "http://localhost:5144"; // Default dev

    public event Action<string, string>? NotificationReceived;
    public event Action<MessageDto>? MessageReceived;

    public SignalRClientService(AppState appState, 
                                Khadamat.Shared.Interfaces.ISecureStorageService secureStorage,
                                NavigationManager navigation)
    {
        _appState = appState;
        _secureStorage = secureStorage;
        _navigation = navigation;
    }

    public async Task StartAsync()
    {
        var token = await _secureStorage.GetAsync("authToken");
        if (string.IsNullOrEmpty(token)) return;

        // Build connections
        _notificationHub = new HubConnectionBuilder()
            .WithUrl($"{_apiBaseUrl}/notificationHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _notificationHub.On<string, string>("ReceiveNotification", (title, message) =>
        {
            NotificationReceived?.Invoke(title, message);
            _appState.HasUnreadNotifications = true; // Simple flag in AppState
            _appState.TriggerStateChanged();
        });

        _chatHub = new HubConnectionBuilder()
            .WithUrl($"{_apiBaseUrl}/chatHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        _chatHub.On<MessageDto>("ReceiveMessage", (message) =>
        {
            MessageReceived?.Invoke(message);
            _appState.HasUnreadMessages = true;
            _appState.TriggerStateChanged();
        });

        try
        {
            await _notificationHub.StartAsync();
            await _chatHub.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting SignalR: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        if (_notificationHub != null) await _notificationHub.StopAsync();
        if (_chatHub != null) await _chatHub.StopAsync();
    }
}
