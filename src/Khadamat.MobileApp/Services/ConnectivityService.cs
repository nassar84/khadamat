using Microsoft.Maui.Networking;
using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services
{
    public class ConnectivityService : IConnectivityService
    {
        private readonly IConnectivity _connectivity;

        public ConnectivityService(IConnectivity connectivity)
        {
            _connectivity = connectivity;
            _connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        public bool IsConnected => _connectivity.NetworkAccess == NetworkAccess.Internet;

        public event EventHandler<bool>? ConnectivityChanged;

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            ConnectivityChanged?.Invoke(this, IsConnected);
        }
    }
}
