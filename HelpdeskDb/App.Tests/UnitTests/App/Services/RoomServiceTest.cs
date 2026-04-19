using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class RoomServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly RoomService _service;

    public RoomServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsRoomsOrderedByName()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var names = results.Select(r => r.RoomName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesCupboardsInRooms()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.All(results, r =>
        {
            Assert.NotNull(r.CupboardsInRooms);
            Assert.Single(r.CupboardsInRooms!);
        });
    }

    private (AppDbContext, RoomService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new RoomService(uow, new RoomBLLMapper());
        return (context, service);
    }
}