using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class CupboardInRoomRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CupboardInRoomRepository _repository;

    public CupboardInRoomRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsAllWithCupboardAndRoom()
    {
        var results = (await _repository.AllAsync()).ToList();

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
        var results = (await _repository.AllAsync()).ToList();

        var codeNames = results.Select(cir => cir.Cupboard!.CodeName).ToList();
        Assert.Equal(codeNames.OrderBy(n => n).ToList(), codeNames);
    }

    [Fact]
    public async Task FindAsync_IncludesCupboardAndRoom()
    {
        var existing = _context.CupboardsInRooms.First();

        var found = await _repository.FindAsync(existing.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.Cupboard);
        Assert.NotNull(found.Room);
    }

    [Fact]
    public async Task GetCupboardInRoomByCupboardId_ReturnsMatch()
    {
        var existing = _context.CupboardsInRooms.First();

        var found = await _repository.GetCupboardInRoomByCupboardId(existing.CupboardId);

        Assert.NotNull(found);
        Assert.Equal(existing.Id, found!.Id);
    }

    [Fact]
    public async Task GetCupboardInRoomByCupboardId_ReturnsNullWhenMissing()
    {
        var found = await _repository.GetCupboardInRoomByCupboardId(Guid.NewGuid());

        Assert.Null(found);
    }

    private (AppDbContext, CupboardInRoomRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new CupboardInRoomRepository(context);
        return (context, repository);
    }
}
