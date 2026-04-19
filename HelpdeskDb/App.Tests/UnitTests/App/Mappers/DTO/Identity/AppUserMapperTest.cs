using App.DTO.v1.CreateObjects.Identity;
using App.DTO.v1.Mappers.Identity;
using V1 = App.DTO.v1.Identity;
using Dom = App.Domain.Identity;

namespace App.Tests.UnitTests.App.Mappers.DTO.Identity;

public class AppUserMapperTest
{
    private readonly AppUserMapper _mapper = new();

    [Fact]
    public void Map_DomainToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Dom.AppUser?)null));

    [Fact]
    public void Map_V1ToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((V1.AppUser?)null));

    [Fact]
    public void Map_DomainToV1_ShouldCopyScalars()
    {
        var src = new Dom.AppUser { Id = Guid.NewGuid(), Username = "bob" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal("bob", dst.Username);
    }

    [Fact]
    public void Map_V1ToDomain_ShouldCopyScalars()
    {
        var src = new V1.AppUser { Id = Guid.NewGuid(), Username = "bob" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal("bob", dst.Username);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new AppUserCreate
        {
            Username = "alice"
        };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal("alice", dst.Username);
    }
}
