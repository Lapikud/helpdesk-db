using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class LocationServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly LocationService _service;

    public LocationServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsLocationsOrderedByName()
    {
        var results = (await _service.AllAsync()).ToList();

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

        var result = (await _service.AllAsync())
            .First(l => l.Id == location.Id);

        Assert.NotNull(result.LocationsInCupboards);
        Assert.NotEmpty(result.LocationsInCupboards!);
        Assert.NotNull(result.LocationsAssetsCollection);
        Assert.Single(result.LocationsAssetsCollection!);
    }

    private (AppDbContext, LocationService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new LocationService(uow, new LocationBLLMapper());
        return (context, service);
    }
}