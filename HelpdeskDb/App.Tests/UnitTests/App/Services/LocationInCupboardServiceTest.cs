using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class LocationInCupboardServiceTest  : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly LocationInCupboardService _service;

    public LocationInCupboardServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsAllWithCupboardAndLocation()
    {
        var results = (await _service.AllAsync()).ToList();

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
        var results = (await _service.AllAsync()).ToList();

        var names = results.Select(lic => lic.Location!.LocationName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task FindAsync_IncludesCupboardAndLocation()
    {
        var existing = _context.LocationsInCupboards.First();

        var found = await _service.FindAsync(existing.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.Cupboard);
        Assert.NotNull(found.Location);
    }

    [Fact]
    public async Task GetLocationInCupboardByLocationId_ReturnsMatch()
    {
        var existing = _context.LocationsInCupboards.First();

        var found = await _service.GetLocationInCupboardByLocationId(existing.LocationId);

        Assert.NotNull(found);
        Assert.Equal(existing.Id, found!.Id);
    }

    [Fact]
    public async Task GetLocationInCupboardByLocationId_ReturnsNullWhenMissing()
    {
        var found = await _service.GetLocationInCupboardByLocationId(Guid.NewGuid());

        Assert.Null(found);
    }

    private (AppDbContext, LocationInCupboardService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new LocationInCupboardService(uow, new LocationInCupboardBLLMapper());
        return (context, service);
    }
}