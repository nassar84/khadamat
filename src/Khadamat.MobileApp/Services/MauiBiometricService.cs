using Khadamat.Shared.Interfaces;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Khadamat.MobileApp.Services;

public class MauiBiometricService : IBiometricService
{
    public async Task<bool> IsAvailableAsync()
    {
        return await CrossFingerprint.Current.IsAvailableAsync();
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
        var request = new AuthenticationRequestConfiguration("المصادقة الحيوية", reason);
        var result = await CrossFingerprint.Current.AuthenticateAsync(request);
        return result.Authenticated;
    }
}
