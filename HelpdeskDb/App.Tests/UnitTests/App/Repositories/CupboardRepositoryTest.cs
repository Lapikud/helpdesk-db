using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class CupboardRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CupboardRepository _repository;

    public CupboardRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsCupboardsOrderedByCodeName()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var codes = results.Select(c => c.CodeName).ToList();
        Assert.Equal(codes.OrderBy(n => n).ToList(), codes);
    }

    [Fact]
    public async Task AllAsync_IncludesLocationsInCupboardsAndCupboardsInRooms()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.All(results, c =>
        {
            Assert.NotNull(c.LocationsInCupboards);
            Assert.Single(c.LocationsInCupboards!);
            Assert.NotNull(c.CupboardsInRooms);
            Assert.Single(c.CupboardsInRooms!);
        });
    }

    private (AppDbContext, CupboardRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new CupboardRepository(context);
        return (context, repository);
    }
}
