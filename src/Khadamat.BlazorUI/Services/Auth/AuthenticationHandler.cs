using System.Net.Http.Headers;
using System.Net.Http.Json;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Shared.Interfaces;

namespace Khadamat.BlazorUI.Services.Auth;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly ISecureStorageService _secureStorage;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

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

        var response = await base.SendAsync(request, cancellationToken);

        // If 401 Unauthorized and not an auth endpoint, attempt automatic token refresh
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
            request.RequestUri != null &&
            !request.RequestUri.AbsolutePath.Contains("/auth/login", StringComparison.OrdinalIgnoreCase) &&
            !request.RequestUri.AbsolutePath.Contains("/auth/refresh-token", StringComparison.OrdinalIgnoreCase))
        {
            var refreshedToken = await TryRefreshTokenAsync(request.RequestUri);
            if (!string.IsNullOrEmpty(refreshedToken))
            {
                // Retry original request with newly refreshed token
                var newRequest = await CloneHttpRequestMessageAsync(request);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshedToken);
                response.Dispose();
                return await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<string?> TryRefreshTokenAsync(Uri originalUri)
    {
        await _refreshLock.WaitAsync();
        try
        {
            var token = await _secureStorage.GetAsync("authToken");
            var refreshToken = await _secureStorage.GetAsync("refreshToken");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return null;

            // Construct base URL from originalUri
            var baseUrl = $"{originalUri.Scheme}://{originalUri.Authority}";
            using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

            var refreshReq = new RefreshTokenRequest
            {
                Token = token,
                RefreshToken = refreshToken
            };

            var refreshResp = await client.PostAsJsonAsync("v1/auth/refresh-token", refreshReq);
            if (refreshResp.IsSuccessStatusCode)
            {
                var result = await refreshResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                if (result?.Success == true && result.Data != null)
                {
                    await _secureStorage.SaveAsync("authToken", result.Data.Token);
                    await _secureStorage.SaveAsync("refreshToken", result.Data.RefreshToken);
                    return result.Data.Token;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthenticationHandler] Token refresh failed: {ex.Message}");
        }
        finally
        {
            _refreshLock.Release();
        }

        return null;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri)
        {
            Version = req.Version
        };

        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in req.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
