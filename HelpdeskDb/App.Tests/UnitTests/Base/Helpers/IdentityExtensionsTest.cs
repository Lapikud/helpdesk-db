using System.Security.Claims;
using Base.Helpers;

namespace App.Tests.UnitTests.Base.Helpers;

public class IdentityExtensionsTest
{
    private const string Key = "ThisIsASecretKeyUsedOnlyForTestingPurposes_MustBeLongEnoughForHmacSha512_0123456789";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    [Fact]
    public void GetUserId_ShouldReturnGuid_WhenClaimPresent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetUserId();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_ShouldThrow_WhenClaimMissing()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_ShouldThrow_WhenClaimValueNotAGuid()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") });
        var principal = new ClaimsPrincipal(identity);

        // Act & Assert
        Assert.Throws<FormatException>(() => principal.GetUserId());
    }

    [Fact]
    public void GenerateJwt_ShouldProduceValidatableToken()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };

        // Act
        var token = IdentityExtensions.GenerateJwt(claims, Key, Issuer, Audience, DateTime.UtcNow.AddMinutes(5));

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(IdentityExtensions.ValidateJwt(token, Key, Issuer, Audience));
    }

    [Fact]
    public void ValidateJwt_ShouldReturnFalse_ForWrongKey()
    {
        var token = IdentityExtensions.GenerateJwt(Array.Empty<Claim>(), Key, Issuer, Audience,
            DateTime.UtcNow.AddMinutes(5));

        var wrongKey = "AnotherWrongKeyThatIsAlsoLongEnoughForHmacSha512_0123456789_0123456789_0123456789";
        Assert.False(IdentityExtensions.ValidateJwt(token, wrongKey, Issuer, Audience));
    }

    [Fact]
    public void ValidateJwt_ShouldReturnFalse_ForWrongIssuer()
    {
        var token = IdentityExtensions.GenerateJwt(Array.Empty<Claim>(), Key, Issuer, Audience,
            DateTime.UtcNow.AddMinutes(5));

        Assert.False(IdentityExtensions.ValidateJwt(token, Key, "wrong-issuer", Audience));
    }

    [Fact]
    public void ValidateJwt_ShouldReturnFalse_ForWrongAudience()
    {
        var token = IdentityExtensions.GenerateJwt(Array.Empty<Claim>(), Key, Issuer, Audience,
            DateTime.UtcNow.AddMinutes(5));

        Assert.False(IdentityExtensions.ValidateJwt(token, Key, Issuer, "wrong-audience"));
    }

    [Fact]
    public void ValidateJwt_ShouldReturnFalse_ForMalformedToken()
    {
        Assert.False(IdentityExtensions.ValidateJwt("not.a.real.jwt", Key, Issuer, Audience));
    }
}
