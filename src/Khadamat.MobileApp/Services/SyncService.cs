using Microsoft.Maui.Networking;
using Khadamat.BlazorUI.Services;
using Khadamat.MobileApp.Services;

namespace Khadamat.MobileApp.Services;

public class SyncService
{
    private readonly Khadamat.Application.Interfaces.IOfflineDataService _localData;
    private readonly ApiClient _api;

    public SyncService(Khadamat.Application.Interfaces.IOfflineDataService localData, ApiClient api)
    {
        _localData = localData;
        _api = api;
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            await SyncPendingActionsAsync();
        }
    }

    public async Task SyncPendingActionsAsync()
    {
        // Placeholder for sync logic
        // Get all pending SyncActions from LocalDataService and push to API
        Console.WriteLine("ANTIGRAVITY_LOG: SyncService - checking for pending actions");
    }
}
