using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services;

public class WebExternalAuthService : IExternalAuthService
{
    public Task<ExternalAuthResult?> AuthenticateAsync(string provider, string authUrl, string callbackScheme)
    {
        // On Web, we usually navigate, so this won't return a result in the same session
        // Return null or throw to indicate it's not handled this way
        return Task.FromResult<ExternalAuthResult?>(new ExternalAuthResult { Error = "REDIRECT_REQUIRED" });
    }
}
