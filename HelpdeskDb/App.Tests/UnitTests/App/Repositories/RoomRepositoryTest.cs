using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class RoomRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly RoomRepository _repository;

    public RoomRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsRoomsOrderedByName()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var names = results.Select(r => r.RoomName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesCupboardsInRooms()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.All(results, r =>
        {
            Assert.NotNull(r.CupboardsInRooms);
            Assert.Single(r.CupboardsInRooms!);
        });
    }

    private (AppDbContext, RoomRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new RoomRepository(context);
        return (context, repository);
    }
}
