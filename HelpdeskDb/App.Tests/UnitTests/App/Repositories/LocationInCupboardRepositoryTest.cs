using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class LocationInCupboardRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly LocationInCupboardRepository _repository;

    public LocationInCupboardRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsAllWithCupboardAndLocation()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, lic =>
        {
            Assert.NotNull(lic.Cupboard);
            Assert.NotNull(lic.Location);
        });
    }

    [Fact]
    public async Task AllAsync_OrdersByLocationName()
    {
        var results = (await _repository.AllAsync()).ToList();

        var names = results.Select(lic => lic.Location!.LocationName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task FindAsync_IncludesCupboardAndLocation()
    {
        var existing = _context.LocationsInCupboards.First();

        var found = await _repository.FindAsync(existing.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.Cupboard);
        Assert.NotNull(found.Location);
    }

    [Fact]
    public async Task GetLocationInCupboardByLocationId_ReturnsMatch()
    {
        var existing = _context.LocationsInCupboards.First();

        var found = await _repository.GetLocationInCupboardByLocationId(existing.LocationId);

        Assert.NotNull(found);
        Assert.Equal(existing.Id, found!.Id);
    }

    [Fact]
    public async Task GetLocationInCupboardByLocationId_ReturnsNullWhenMissing()
    {
        var found = await _repository.GetLocationInCupboardByLocationId(Guid.NewGuid());

        Assert.Null(found);
    }

    private (AppDbContext, LocationInCupboardRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new LocationInCupboardRepository(context);
        return (context, repository);
    }
}
