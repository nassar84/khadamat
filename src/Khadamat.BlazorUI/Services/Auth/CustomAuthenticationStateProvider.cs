using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Khadamat.Application.DTOs; // Ensure DTOs are available
using Microsoft.AspNetCore.Components.Authorization;
using Khadamat.Shared.Interfaces; // Add this using directive for ISecureStorageService

namespace Khadamat.BlazorUI.Services.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly Khadamat.Shared.Interfaces.ISecureStorageService _secureStorage;
    
    public CustomAuthenticationStateProvider(Khadamat.Shared.Interfaces.ISecureStorageService secureStorage)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: CustomAuthenticationStateProvider Constructor called");
        _secureStorage = secureStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Console.WriteLine("ANTIGRAVITY_LOG: GetAuthenticationStateAsync started");
        try {
            string? token = await _secureStorage.GetAsync("authToken");
            Console.WriteLine($"ANTIGRAVITY_LOG: Token fetch complete. Found: {!string.IsNullOrEmpty(token)}");
        
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            Console.WriteLine("ANTIGRAVITY_LOG: HttpClient Authorization header will be handled by AuthenticationHandler");

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt", ClaimTypes.Name, ClaimTypes.Role)));
        } catch (Exception ex) {
            Console.WriteLine($"ANTIGRAVITY_LOG: GetAuthenticationStateAsync ERROR: {ex}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        Console.WriteLine("ANTIGRAVITY_LOG: MarkUserAsAuthenticated called");
        
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt", ClaimTypes.Name, ClaimTypes.Role));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        Console.WriteLine("ANTIGRAVITY_LOG: MarkUserAsLoggedOut called");
        
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        try {
            if (string.IsNullOrEmpty(jwt) || !jwt.Contains(".")) {
                Console.WriteLine("ANTIGRAVITY_LOG: Invalid JWT format detected.");
                return claims;
            }
            var parts = jwt.Split('.');
            if (parts.Length < 2) return claims;
            
            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            using var doc = JsonDocument.Parse(jsonBytes);

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                var key = property.Name;
                var elem = property.Value;

                if (key.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) || 
                    key.Equals("role", StringComparison.OrdinalIgnoreCase) || 
                    key.Equals("roles", StringComparison.OrdinalIgnoreCase))
                {
                    if (elem.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in elem.EnumerateArray())
                        {
                            var roleVal = r.GetString();
                            if (!string.IsNullOrWhiteSpace(roleVal))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleVal));
                                claims.Add(new Claim("role", roleVal));
                            }
                        }
                    }
                    else if (elem.ValueKind == JsonValueKind.String)
                    {
                        var roleVal = elem.GetString();
                        if (!string.IsNullOrWhiteSpace(roleVal))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roleVal));
                            claims.Add(new Claim("role", roleVal));
                        }
                    }
                }
                else if (key.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("unique_name", StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    var nameVal = elem.GetString();
                    if (!string.IsNullOrWhiteSpace(nameVal))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, nameVal));
                        claims.Add(new Claim("name", nameVal));
                    }
                }
                else if (key.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("sub", StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("nameid", StringComparison.OrdinalIgnoreCase))
                {
                    var subVal = elem.GetString();
                    if (!string.IsNullOrWhiteSpace(subVal))
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, subVal));
                        claims.Add(new Claim("sub", subVal));
                    }
                }
                else
                {
                    claims.Add(new Claim(key, elem.ToString() ?? ""));
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"ANTIGRAVITY_LOG: ParseClaimsFromJwt Error: {ex.Message}");
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
