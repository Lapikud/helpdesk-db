using App.DAL.EF;
using App.DAL.EF.Repositories.Identity;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class AppUserRoleRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly AppUserRoleRepository _repository;

    public AppUserRoleRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_IncludesUserAndRole()
    {
        var results = (await _repository.AllAsync()).ToList();

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
        var result = await _repository.FindAsync(TestDatabaseFixture.AppUserRoleId);

        Assert.NotNull(result);
        Assert.NotNull(result!.User);
        Assert.NotNull(result.Role);
        Assert.Equal(TestDatabaseFixture.UserId, result.UserId);
        Assert.Equal(TestDatabaseFixture.RoleId, result.RoleId);
    }

    private (AppDbContext, AppUserRoleRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new AppUserRoleRepository(context);
        return (context, repository);
    }
}
