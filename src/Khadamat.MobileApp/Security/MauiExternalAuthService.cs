using Khadamat.Shared.Interfaces;
using Microsoft.Maui.Authentication;

namespace Khadamat.MobileApp.Security;

public class MauiExternalAuthService : IExternalAuthService
{
    public async Task<ExternalAuthResult?> AuthenticateAsync(string provider, string authUrl, string callbackScheme)
    {
        try
        {
            var schemeUri = callbackScheme.Contains("://") 
                ? new Uri(callbackScheme) 
                : new Uri($"{callbackScheme}://");

            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                schemeUri);

            return new ExternalAuthResult
            {
                Token = authResult.Properties.ContainsKey("token") ? authResult.Properties["token"] : null,
                RefreshToken = authResult.Properties.ContainsKey("refreshToken") ? authResult.Properties["refreshToken"] : null
            };
        }
        catch (OperationCanceledException)
        {
            return null; // User cancelled
        }
        catch (Exception ex)
        {
            return new ExternalAuthResult { Error = ex.Message };
        }
    }
}
