using App.DTO.v1.Mappers.Identity;
using V1 = App.DTO.v1.Identity;
using Dom = App.Domain.Identity;

namespace App.Tests.UnitTests.App.Mappers.DTO.Identity;

public class RefreshTokensMapperTest
{
    private readonly RefreshTokensMapper _mapper = new();

    [Fact]
    public void Map_DomainToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Dom.AppRefreshToken?)null));

    [Fact]
    public void Map_V1ToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((V1.AppRefreshToken?)null));

    [Fact]
    public void Map_DomainToV1_ShouldCopyScalarsButRedactTokenValues()
    {
        var src = new Dom.AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
            RefreshToken = "a", PreviousRefreshToken = "b",
            Expiration = DateTime.UtcNow, PreviousExpiration = DateTime.UtcNow.AddMinutes(-1)
        };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.UserId, dst.UserId);
        // Token strings must never be emitted outward - they are redacted so a leaked/logged
        // response can't be used to hijack the session.
        Assert.NotEqual("a", dst.RefreshToken);
        Assert.NotEqual("b", dst.PreviousRefreshToken);
        Assert.Equal(src.Expiration, dst.Expiration);
        Assert.Equal(src.PreviousExpiration, dst.PreviousExpiration);
    }

    [Fact]
    public void Map_DomainToV1_NullPreviousTokenStaysNull()
    {
        var src = new Dom.AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
            RefreshToken = "a", PreviousRefreshToken = null,
        };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.PreviousRefreshToken);
    }

    [Fact]
    public void Map_V1ToDomain_ShouldCopyScalars()
    {
        var src = new V1.AppRefreshToken
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(),
            RefreshToken = "a", PreviousRefreshToken = "b",
            Expiration = DateTime.UtcNow, PreviousExpiration = DateTime.UtcNow.AddMinutes(-1)
        };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.RefreshToken, dst.RefreshToken);
    }
}
