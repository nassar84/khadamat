using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Khadamat.Infrastructure.Identity;

public enum JwtKeyStatus
{
    Active,
    GracePeriod,
    Retired
}

public class JwtKeyEntry
{
    public string Id { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public JwtKeyStatus Status { get; set; } = JwtKeyStatus.Active;
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime? RetiresAtUtc { get; set; }

    public SymmetricSecurityKey ToSecurityKey()
    {
        var keyBytes = Encoding.UTF8.GetBytes(Secret);
        return new SymmetricSecurityKey(keyBytes) { KeyId = Id };
    }
}

public interface IJwtKeyManager
{
    SigningCredentials GetSigningCredentials();
    string GetActiveKeyId();
    IEnumerable<SecurityKey> GetValidationKeys();
    SecurityKey? ResolveSigningKey(string? kid);
    TokenValidationParameters CreateTokenValidationParameters(string? issuer = null, string? audience = null, bool validateLifetime = true);
    DateTime? GetLegacyTokensCutoffUtc();
    bool AreLegacyTokensAllowed();
}

public class JwtKeyManager : IJwtKeyManager
{
    private readonly List<JwtKeyEntry> _keyRing = new();
    private readonly JwtKeyEntry _activeKey;
    private readonly string? _defaultIssuer;
    private readonly string? _defaultAudience;
    private readonly DateTime? _legacyTokensCutoffUtc;
    private readonly HashSet<string> _revokedTokenIds = new(StringComparer.OrdinalIgnoreCase);

    public JwtKeyManager(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");
        _defaultIssuer = jwtSection["Issuer"];
        _defaultAudience = jwtSection["Audience"];

        // Configurable cutoff for legacy tokens without kid (sunset period)
        if (DateTime.TryParse(jwtSection["LegacyTokensCutoffUtc"], out var cutoff))
        {
            _legacyTokensCutoffUtc = cutoff.ToUniversalTime();
        }
        else
        {
            // Default: 30 days transition grace period from initial deployment
            _legacyTokensCutoffUtc = DateTime.UtcNow.AddDays(30);
        }

        // 1. Check for explicit multi-key configuration under JwtSettings:Keys
        var keysSection = jwtSection.GetSection("Keys").GetChildren();
        foreach (var section in keysSection)
        {
            var id = section["Id"] ?? section.Key;
            var secret = section["Secret"];
            if (string.IsNullOrWhiteSpace(secret)) continue;

            var statusStr = section["Status"] ?? "Active";
            if (!Enum.TryParse<JwtKeyStatus>(statusStr, true, out var status))
            {
                status = JwtKeyStatus.Active;
            }

            _keyRing.Add(new JwtKeyEntry
            {
                Id = id,
                Secret = secret,
                Status = status
            });
        }

        // 2. Fallback to primary JwtSettings:Secret for full backward compatibility
        var primarySecret = jwtSection["Secret"];
        if (!string.IsNullOrWhiteSpace(primarySecret) && !_keyRing.Any(k => k.Secret == primarySecret))
        {
            var activeId = jwtSection["ActiveKeyId"] ?? "legacy-primary";
            _keyRing.Add(new JwtKeyEntry
            {
                Id = activeId,
                Secret = primarySecret,
                Status = JwtKeyStatus.Active
            });
        }

        // 3. Ensure at least one active key exists for signing
        _activeKey = _keyRing.FirstOrDefault(k => k.Status == JwtKeyStatus.Active)
            ?? _keyRing.FirstOrDefault()
            ?? throw new InvalidOperationException("No valid JWT signing key found in configuration (JwtSettings:Secret or JwtSettings:Keys).");
    }

    public SigningCredentials GetSigningCredentials()
    {
        var key = _activeKey.ToSecurityKey();
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GetActiveKeyId() => _activeKey.Id;

    public DateTime? GetLegacyTokensCutoffUtc() => _legacyTokensCutoffUtc;

    public bool AreLegacyTokensAllowed()
    {
        if (!_legacyTokensCutoffUtc.HasValue) return true;
        return DateTime.UtcNow <= _legacyTokensCutoffUtc.Value;
    }

    public IEnumerable<SecurityKey> GetValidationKeys()
    {
        return _keyRing
            .Where(k => k.Status == JwtKeyStatus.Active || k.Status == JwtKeyStatus.GracePeriod)
            .Select(k => k.ToSecurityKey());
    }

    public SecurityKey? ResolveSigningKey(string? kid)
    {
        if (string.IsNullOrWhiteSpace(kid))
        {
            // Legacy token without kid in header: check whether legacy acceptance has expired
            if (!AreLegacyTokensAllowed())
            {
                // Sunset cutoff reached: reject tokens without kid
                return null;
            }

            return _activeKey.ToSecurityKey();
        }

        var entry = _keyRing.FirstOrDefault(k => string.Equals(k.Id, kid, StringComparison.OrdinalIgnoreCase));
        if (entry == null || entry.Status == JwtKeyStatus.Retired)
        {
            // Key not found or explicitly retired -> reject
            return null;
        }

        return entry.ToSecurityKey();
    }

    public TokenValidationParameters CreateTokenValidationParameters(string? issuer = null, string? audience = null, bool validateLifetime = true)
    {
        var validationKeys = GetValidationKeys().ToList();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer ?? _defaultIssuer),
            ValidIssuer = issuer ?? _defaultIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(audience ?? _defaultAudience),
            ValidAudience = audience ?? _defaultAudience,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            // Algorithm allow-list to prevent algorithm confusion attacks
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            // Tight clock skew tolerance (1 minute instead of default 5 minutes)
            ClockSkew = TimeSpan.FromMinutes(1),
            IssuerSigningKeys = validationKeys,
            IssuerSigningKeyResolver = (token, securityToken, kid, validationParams) =>
            {
                var resolved = ResolveSigningKey(kid);
                return resolved != null ? new[] { resolved } : Enumerable.Empty<SecurityKey>();
            }
        };

        if (validationKeys.Count > 0)
        {
            parameters.IssuerSigningKey = validationKeys.First();
        }

        return parameters;
    }
}
