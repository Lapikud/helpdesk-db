using App.BLL.Mappers.Identity;
using BllDto = App.BLL.DTO.Identity;
using DalDto = App.DAL.DTO.Identity;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class AppUserRoleBLLMapperTest
{
    private readonly AppUserRoleBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.AppUserRole?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.AppUserRole?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.AppUserRole { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.RoleId, dst.RoleId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedUserAndRole_ShouldProject()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var src = new DalDto.AppUserRole
        {
            Id = Guid.NewGuid(), UserId = userId, RoleId = roleId,
            User = new DalDto.AppUser { Id = userId, Username = "bob" },
            Role = new DalDto.AppRole { Id = roleId, Name = "admins" }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.User);
        Assert.Equal(userId, dst.User!.Id);
        Assert.Equal("bob", dst.User.Username);
        Assert.NotNull(dst.Role);
        Assert.Equal(roleId, dst.Role!.Id);
        Assert.Equal("admins", dst.Role.Name);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.AppUserRole { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.User);
        Assert.Null(dst.Role);
    }
}
