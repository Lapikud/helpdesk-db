using App.DAL.EF;
using App.DAL.EF.Repositories;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class CategoryRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task AllAsync_ReturnsCategoriesOrderedByName()
    {
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "Abcdefg", Comment = "comment" };
        _context.Assets.Add(asset);
        _context.CategoryAssetsCollection.Add(new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            CategoryId = _context.Categories.First().Id,
        });
        _context.SaveChanges();

        var results = (await _repository.AllAsync()).ToList();

        Assert.NotEmpty(results);
        var names = results.Select(c => c.CategoryName).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task AllAsync_IncludesCategoryAssetsCollection()
    {
        var asset = new Domain.Asset { Id = Guid.NewGuid(), AssetName = "Abcdefg", Comment = "comment" };
        _context.Assets.Add(asset);
        var category = _context.Categories.First();
        _context.CategoryAssetsCollection.Add(new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            CategoryId = category.Id,
        });
        _context.SaveChanges();

        var result = (await _repository.AllAsync())
            .First(c => c.Id == category.Id);

        Assert.NotNull(result.CategoryAssetsCollection);
        Assert.Single(result.CategoryAssetsCollection!);
    }

    private (AppDbContext, CategoryRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new CategoryRepository(context);
        return (context, repository);
    }
}
