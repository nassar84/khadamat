using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Khadamat.Application.DTOs; // Ensure DTOs are available
using Microsoft.AspNetCore.Components.Authorization;

namespace Khadamat.BlazorUI.Services.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: CustomAuthenticationStateProvider Constructor called");
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Console.WriteLine("ANTIGRAVITY_LOG: GetAuthenticationStateAsync started");
        try {
            // Safety timeout for local storage in MAUI (sometimes the bridge hangs)
            var tokenTask = _localStorage.GetItemAsync<string>("authToken").AsTask();
            var completedTask = await Task.WhenAny(tokenTask, Task.Delay(2000));
            
            string? token = null;
            if (completedTask == tokenTask) {
                token = await tokenTask;
                Console.WriteLine($"ANTIGRAVITY_LOG: Token fetch complete. Found: {!string.IsNullOrEmpty(token)}");
            } else {
                Console.WriteLine("ANTIGRAVITY_LOG: Token fetch TIMEOUT (2s). Proceeding as Anonymous.");
            }
        
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        } catch (Exception ex) {
            Console.WriteLine($"ANTIGRAVITY_LOG: GetAuthenticationStateAsync ERROR: {ex}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        if (string.IsNullOrEmpty(jwt) || !jwt.Contains(".")) {
            Console.WriteLine("ANTIGRAVITY_LOG: Invalid JWT format detected.");
            return claims;
        }
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;
        
        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            // Handle Roles (try both short and long names)
            object? roles = null;
            if (!keyValuePairs.TryGetValue(ClaimTypes.Role, out roles))
            {
                keyValuePairs.TryGetValue("role", out roles);
            }

            if (roles != null)
            {
                var rolesStr = roles.ToString()!.Trim();
                if (rolesStr.StartsWith("["))
                {
                    try
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(rolesStr);
                        foreach (var parsedRole in parsedRoles!)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                        }
                    }
                    catch
                    {
                        // Fallback if not a valid JSON array
                        claims.Add(new Claim(ClaimTypes.Role, rolesStr));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, rolesStr));
                }
                
                keyValuePairs.Remove(ClaimTypes.Role);
                keyValuePairs.Remove("role");
            }

            // Handle Names (try both short and long names)
            object? name = null;
            if (!keyValuePairs.TryGetValue(ClaimTypes.Name, out name))
            {
                keyValuePairs.TryGetValue("unique_name", out name);
            }
            if (name != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                keyValuePairs.Remove(ClaimTypes.Name);
                keyValuePairs.Remove("unique_name");
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
