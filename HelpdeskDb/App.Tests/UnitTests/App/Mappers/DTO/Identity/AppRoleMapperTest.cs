using App.DTO.v1.CreateObjects.Identity;
using App.DTO.v1.Identity;
using App.DTO.v1.Mappers.Identity;

namespace App.Tests.UnitTests.App.Mappers.DTO.Identity;

public class AppRoleMapperTest
{
    private readonly AppRoleMapper _mapper = new();

    [Fact]
    public void Map_DomainToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Identity.AppRole?)null));

    [Fact]
    public void Map_V1ToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((AppRole?)null));

    [Fact]
    public void Map_DomainToV1_ShouldCopyScalars()
    {
        var src = new Domain.Identity.AppRole { Id = Guid.NewGuid(), Name = "admins" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal("admins", dst.Name);
    }

    [Fact]
    public void Map_V1ToDomain_ShouldCopyScalars()
    {
        var src = new AppRole { Id = Guid.NewGuid(), Name = "members" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal("members", dst.Name);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new AppRoleCreate() { Name = "pixels" };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal("pixels", dst.Name);
    }
}
