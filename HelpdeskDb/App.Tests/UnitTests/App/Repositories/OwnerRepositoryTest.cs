using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class OwnerRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly OwnerRepository _repository;

    public OwnerRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsOwnersOrderedByName()
    {
        var results = (await _repository.AllAsync()).ToList();

        Assert.Equal(2, results.Count);
        var names = results.Select(o => o.OwnerName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesOwnerAssets()
    {
        var owner = _context.Owners.First();
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "Abcdefg", Comment = "comment" };
        _context.Assets.Add(asset);
        _context.OwnerAssets.Add(new Domain.OwnerAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            OwnerId = owner.Id,
        });
        _context.SaveChanges();

        var result = (await _repository.AllAsync()).First(o => o.Id == owner.Id);

        Assert.NotNull(result.OwnerAssets);
        Assert.Single(result.OwnerAssets!);
    }

    private (AppDbContext, OwnerRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new OwnerRepository(context);
        return (context, repository);
    }
}
