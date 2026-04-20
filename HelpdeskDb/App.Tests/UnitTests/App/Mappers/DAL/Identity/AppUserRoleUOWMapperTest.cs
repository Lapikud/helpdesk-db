using App.DAL.EF.Mappers.Identity;
using DalDto = App.DAL.DTO.Identity;
using DomainId = App.Domain.Identity;

namespace App.Tests.UnitTests.App.Mappers.DAL.Identity;

public class AppUserRoleUOWMapperTest
{
    private readonly AppUserRoleUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((DomainId.AppUserRole?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.AppUserRole?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DomainId.AppUserRole
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.RoleId, dst.RoleId);
        Assert.Null(dst.User);
        Assert.Null(dst.Role);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProject()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var src = new DomainId.AppUserRole
        {
            Id = Guid.NewGuid(), UserId = userId, RoleId = roleId,
            User = new DomainId.AppUser { Id = userId, Username = "bob" },
            Role = new DomainId.AppRole { Id = roleId, Name = "admins" }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.User);
        Assert.Equal(userId, dst.User!.Id);
        Assert.Equal("bob", dst.User.Username);
        Assert.NotNull(dst.Role);
        Assert.Equal(roleId, dst.Role!.Id);
        Assert.Equal("admins", dst.Role.Name);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.AppUserRole
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.RoleId, dst.RoleId);
        Assert.Null(dst.User);
        Assert.Null(dst.Role);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProject()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var src = new DalDto.AppUserRole
        {
            Id = Guid.NewGuid(), UserId = userId, RoleId = roleId,
            User = new DalDto.AppUser { Id = userId, Username = "alice" },
            Role = new DalDto.AppRole { Id = roleId, Name = "members" }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.User);
        Assert.Equal("alice", dst.User!.Username);
        Assert.NotNull(dst.Role);
        Assert.Equal("members", dst.Role!.Name);
    }
}
