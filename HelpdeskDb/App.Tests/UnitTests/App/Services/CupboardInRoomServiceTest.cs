using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class CupboardInRoomServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CupboardInRoomService _service;

    public CupboardInRoomServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsAllWithCupboardAndRoom()
    {
        var results = (await _service.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, cir =>
        {
            Assert.NotNull(cir.Cupboard);
            Assert.NotNull(cir.Room);
        });
    }

    [Fact]
    public async Task AllAsync_OrdersByCupboardCodeName()
    {
        var results = (await _service.AllAsync()).ToList();

        var codeNames = results.Select(cir => cir.Cupboard!.CodeName).ToList();
        Assert.Equal(codeNames.OrderBy(n => n).ToList(), codeNames);
    }

    [Fact]
    public async Task FindAsync_IncludesCupboardAndRoom()
    {
        var existing = _context.CupboardsInRooms.First();

        var found = await _service.FindAsync(existing.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.Cupboard);
        Assert.NotNull(found.Room);
    }

    [Fact]
    public async Task GetCupboardInRoomByCupboardId_ReturnsMatch()
    {
        var existing = _context.CupboardsInRooms.First();

        var found = await _service.GetCupboardInRoomByCupboardId(existing.CupboardId);

        Assert.NotNull(found);
        Assert.Equal(existing.Id, found!.Id);
    }

    [Fact]
    public async Task GetCupboardInRoomByCupboardId_ReturnsNullWhenMissing()
    {
        var found = await _service.GetCupboardInRoomByCupboardId(Guid.NewGuid());

        Assert.Null(found);
    }

    private (AppDbContext, CupboardInRoomService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new CupboardInRoomService(uow, new CupboardInRoomBLLMapper());
        return (context, service);
    }
}