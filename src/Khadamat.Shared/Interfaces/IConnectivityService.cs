using System;

namespace Khadamat.Shared.Interfaces
{
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        event EventHandler<bool> ConnectivityChanged;
    }
}
