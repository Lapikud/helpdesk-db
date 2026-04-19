using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

public class CategoryServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CategoryService _service;

    public CategoryServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsCategoriesOrderedByName()
    {
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "Abcdef", Comment = "comment" };
        _context.Assets.Add(asset);
        _context.CategoryAssetsCollection.Add(new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            CategoryId = _context.Categories.First().Id,
        });
        _context.SaveChanges();

        var results = (await _service.AllAsync()).ToList();

        Assert.NotEmpty(results);
        var names = results.Select(c => c.CategoryName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesCategoryAssetsCollection()
    {
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "A", Comment = "c" };
        _context.Assets.Add(asset);
        var category = _context.Categories.First();
        _context.CategoryAssetsCollection.Add(new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            CategoryId = category.Id,
        });
        _context.SaveChanges();

        var result = (await _service.AllAsync())
            .First(c => c.Id == category.Id);

        Assert.NotNull(result.CategoryAssetsCollection);
        Assert.Single(result.CategoryAssetsCollection!);
    }

    private (AppDbContext, CategoryService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new CategoryService(uow, new CategoryBLLMapper());
        return (context, service);
    }
}