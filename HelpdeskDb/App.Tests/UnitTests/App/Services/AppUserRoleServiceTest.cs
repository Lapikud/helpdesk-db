using App.BLL.Mappers.Identity;
using App.BLL.Services.Identity;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class AppUserRoleServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly AppUserRoleService _service;

    public AppUserRoleServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_IncludesUserAndRole()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.NotEmpty(results);
        Assert.All(results, ur =>
        {
            Assert.NotNull(ur.User);
            Assert.NotNull(ur.Role);
        });
    }

    [Fact]
    public async Task FindAsync_IncludesUserAndRole()
    {
        var result = await _service.FindAsync(TestDatabaseFixture.AppUserRoleId);

        Assert.NotNull(result);
        Assert.NotNull(result!.User);
        Assert.NotNull(result.Role);
        Assert.Equal(TestDatabaseFixture.UserId, result.UserId);
        Assert.Equal(TestDatabaseFixture.RoleId, result.RoleId);
    }

    private (AppDbContext, AppUserRoleService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new AppUserRoleService(uow, new AppUserRoleBLLMapper());

        return (context, service);
    }
}