using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class CupboardServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CupboardService _service;

    public CupboardServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsCupboardsOrderedByCodeName()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var codes = results.Select(c => c.CodeName).ToList();
        Assert.Equal(codes.OrderBy(n => n).ToList(), codes);
    }

    [Fact]
    public async Task AllAsync_IncludesLocationsInCupboardsAndCupboardsInRooms()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.All(results, c =>
        {
            Assert.NotNull(c.LocationsInCupboards);
            Assert.Single(c.LocationsInCupboards!);
            Assert.NotNull(c.CupboardsInRooms);
            Assert.Single(c.CupboardsInRooms!);
        });
    }

    private (AppDbContext, CupboardService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new CupboardService(uow, new CupboardBLLMapper());
        return (context, service);
    }
}