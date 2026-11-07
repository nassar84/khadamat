using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Khadamat.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Khadamat.UnitTests;

public class JwtKeyRotationTests
{
    private const string Issuer = "KhadamatAppTest";
    private const string Audience = "KhadamatUsersTest";
    private const string KeyV1 = "SecretKey_Version1_Minimum32CharactersRequired!";
    private const string KeyV2 = "SecretKey_Version2_Minimum32CharactersRequired!";
    private const string KeyRetired = "SecretKey_Retired_Minimum32CharactersRequired!";

    private IConfiguration CreateRotationConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:Issuer", Issuer},
            {"JwtSettings:Audience", Audience},
            {"JwtSettings:ExpiryMinutes", "60"},
            // Key ring with New Active key (KeyV2), GracePeriod key (KeyV1), and Retired key
            {"JwtSettings:Keys:0:Id", "key-v2-active"},
            {"JwtSettings:Keys:0:Secret", KeyV2},
            {"JwtSettings:Keys:0:Status", "Active"},

            {"JwtSettings:Keys:1:Id", "key-v1-grace"},
            {"JwtSettings:Keys:1:Secret", KeyV1},
            {"JwtSettings:Keys:1:Status", "GracePeriod"},

            {"JwtSettings:Keys:2:Id", "key-v0-retired"},
            {"JwtSettings:Keys:2:Secret", KeyRetired},
            {"JwtSettings:Keys:2:Status", "Retired"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private string CreateToken(string secret, string? kid, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        if (!string.IsNullOrEmpty(kid))
        {
            key.KeyId = kid;
        }

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-test-123"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "User")
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public void Test1_NewToken_Validation_Succeeds()
    {
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        // Token signed with the new active key (key-v2-active)
        var token = CreateToken(KeyV2, "key-v2-active", DateTime.UtcNow.AddMinutes(30));

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
        Assert.Equal("user-test-123", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void Test2_OldToken_DuringGracePeriod_Validation_Succeeds_WithoutUserLogout()
    {
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        // User logged in before rotation, holding a token signed with KeyV1 (GracePeriod)
        var oldToken = CreateToken(KeyV1, "key-v1-grace", DateTime.UtcNow.AddMinutes(30));

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(oldToken, validationParams, out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
        Assert.Equal("user-test-123", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void Test3_LegacyToken_WithoutKid_DuringGracePeriod_Validation_Succeeds()
    {
        // Backward compatibility: token without kid header should still validate against active/grace keys
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        var tokenWithoutKid = CreateToken(KeyV2, null, DateTime.UtcNow.AddMinutes(30));

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(tokenWithoutKid, validationParams, out var validatedToken);

        Assert.NotNull(principal);
        Assert.Equal("user-test-123", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void Test4_RetiredKey_Token_Validation_Fails()
    {
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        // Token signed with a retired key (key-v0-retired)
        var retiredToken = CreateToken(KeyRetired, "key-v0-retired", DateTime.UtcNow.AddMinutes(30));

        var handler = new JwtSecurityTokenHandler();
        Assert.ThrowsAny<SecurityTokenException>(() =>
        {
            handler.ValidateToken(retiredToken, validationParams, out _);
        });
    }

    [Fact]
    public void Test5_InvalidSignature_Token_Validation_Fails()
    {
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        // Token signed with an unrecognized/attacker key
        var maliciousKey = "MaliciousAttackerKey_Minimum32CharactersLong!";
        var invalidToken = CreateToken(maliciousKey, "key-v2-active", DateTime.UtcNow.AddMinutes(30));

        var handler = new JwtSecurityTokenHandler();
        Assert.ThrowsAny<SecurityTokenException>(() =>
        {
            handler.ValidateToken(invalidToken, validationParams, out _);
        });
    }

    [Fact]
    public void Test6_ExpiredToken_Validation_Fails()
    {
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        // Token expired 10 minutes ago
        var expiredToken = CreateToken(KeyV2, "key-v2-active", DateTime.UtcNow.AddMinutes(-10));

        var handler = new JwtSecurityTokenHandler();
        Assert.Throws<SecurityTokenExpiredException>(() =>
        {
            handler.ValidateToken(expiredToken, validationParams, out _);
        });
    }

    [Fact]
    public void Test7_SeamlessKeyRotation_Demonstrates_NoUnintendedLogout()
    {
        // Scenario: Server rotates from V1 to V2
        // Both tokens can be validated concurrently by the same server instance
        var config = CreateRotationConfiguration();
        var keyManager = new JwtKeyManager(config);
        var validationParams = keyManager.CreateTokenValidationParameters();

        var oldUserToken = CreateToken(KeyV1, "key-v1-grace", DateTime.UtcNow.AddMinutes(15));
        var newUserToken = CreateToken(KeyV2, "key-v2-active", DateTime.UtcNow.AddMinutes(60));

        var handler = new JwtSecurityTokenHandler();

        var oldPrincipal = handler.ValidateToken(oldUserToken, validationParams, out _);
        var newPrincipal = handler.ValidateToken(newUserToken, validationParams, out _);

        Assert.NotNull(oldPrincipal);
        Assert.NotNull(newPrincipal);
        Assert.Equal(oldPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                     newPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}
