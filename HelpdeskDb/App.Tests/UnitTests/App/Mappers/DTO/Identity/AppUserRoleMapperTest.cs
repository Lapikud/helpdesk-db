using App.DTO.v1.CreateObjects.Identity;
using App.DTO.v1.Mappers.Identity;
using V1 = App.DTO.v1.Identity;
using Dom = App.Domain.Identity;

namespace App.Tests.UnitTests.App.Mappers.DTO.Identity;

public class AppUserRoleMapperTest
{
    private readonly AppUserRoleMapper _mapper = new();

    [Fact]
    public void Map_DomainToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Dom.AppUserRole?)null));

    [Fact]
    public void Map_V1ToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((V1.AppUserRole?)null));

    [Fact]
    public void Map_DomainToV1_ShouldCopyScalars()
    {
        var src = new Dom.AppUserRole { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.RoleId, dst.RoleId);
    }

    [Fact]
    public void Map_V1ToDomain_ShouldCopyScalars()
    {
        var src = new V1.AppUserRole { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new AppUserRoleCreate { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.UserId, dst.UserId);
        Assert.Equal(create.RoleId, dst.RoleId);
    }
}
