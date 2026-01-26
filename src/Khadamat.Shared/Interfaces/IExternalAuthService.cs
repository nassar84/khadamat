namespace Khadamat.Shared.Interfaces;

public interface IExternalAuthService
{
    Task<ExternalAuthResult?> AuthenticateAsync(string provider, string authUrl, string callbackScheme);
}

public class ExternalAuthResult
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Error { get; set; }
}
