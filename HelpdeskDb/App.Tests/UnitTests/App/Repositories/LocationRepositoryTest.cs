using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class LocationRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly LocationRepository _repository;

    public LocationRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsLocationsOrderedByName()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var names = results.Select(l => l.LocationName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesLocationsInCupboardsAndLocationAssets()
    {
        var location = _context.Locations.First();
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "Abcdefg", Comment = "comment" };
        _context.Assets.Add(asset);
        _context.LocationAssetsCollection.Add(new Domain.LocationAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            LocationId = location.Id,
        });
        _context.SaveChanges();

        var result = (await _repository.AllAsync())
            .First(l => l.Id == location.Id);

        Assert.NotNull(result.LocationsInCupboards);
        Assert.NotEmpty(result.LocationsInCupboards!);
        Assert.NotNull(result.LocationsAssetsCollection);
        Assert.Single(result.LocationsAssetsCollection!);
    }

    private (AppDbContext, LocationRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new LocationRepository(context);
        return (context, repository);
    }
}
