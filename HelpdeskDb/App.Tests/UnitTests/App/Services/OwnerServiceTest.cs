using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class OwnerServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly OwnerService _service;

    public OwnerServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsOwnersOrderedByName()
    {
        var results = (await _service.AllAsync()).ToList();

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

        var result = (await _service.AllAsync()).First(o => o.Id == owner.Id);

        Assert.NotNull(result.OwnerAssets);
        Assert.Single(result.OwnerAssets!);
    }

    private (AppDbContext, OwnerService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new OwnerService(uow, new OwnerBLLMapper());
        return (context, service);
    }
}