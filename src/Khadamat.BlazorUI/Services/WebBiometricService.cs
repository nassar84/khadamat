using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services;

public class WebBiometricService : IBiometricService
{
    public Task<bool> IsAvailableAsync() => Task.FromResult(false);
    public Task<bool> AuthenticateAsync(string reason) => Task.FromResult(false);
}
