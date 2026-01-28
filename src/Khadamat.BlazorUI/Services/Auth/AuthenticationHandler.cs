using System.Net.Http.Headers;
using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services.Auth;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly ISecureStorageService _secureStorage;

    public AuthenticationHandler(ISecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _secureStorage.GetAsync("authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
