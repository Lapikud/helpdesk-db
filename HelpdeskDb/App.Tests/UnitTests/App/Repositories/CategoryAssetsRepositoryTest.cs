using App.DAL.EF;
using App.DAL.EF.Repositories;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class CategoryAssetsRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly CategoryAssetsRepository _repository;
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";
    private const string AssetComment1 = "TestAssetComment1";
    private const string AssetComment2 = "TestAssetComment2";
    private const string CategoryName1 = "TestCategory1";
    private const string CategoryName2 = "TestCategory2";

    public CategoryAssetsRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }
    
    [Fact]
    public async Task AllAsync_ShouldReturnAllCategoryAssets()
    {
        // Arrange
        var categoryAssets = CreateAssetsAndCategoryAssets();
        var categoryAsset1 = categoryAssets.First(ca => ca.Category!.CategoryName == CategoryName1);
        var categoryAsset2 = categoryAssets.First(ca => ca.Category!.CategoryName == CategoryName2);

        // Act
        var categoryAssetsFromRepository = (await _repository.AllAsync()).ToList();

        // Assert
        Assert.NotEmpty(categoryAssetsFromRepository);
        Assert.Equal(2, categoryAssetsFromRepository.Count);
        
        var categoryAssetFromRepo1 = categoryAssetsFromRepository.First(ca => ca.Category!.CategoryName == CategoryName1);
        var categoryAssetFromRepo2 = categoryAssetsFromRepository.First(ca => ca.Category!.CategoryName == CategoryName2);
        
        // CategoryAsset 1
        Assert.Equal(categoryAsset1.Id, categoryAssetFromRepo1.Id);
        Assert.Equal(categoryAsset1.AssetId, categoryAssetFromRepo1.AssetId);
        Assert.Equal(categoryAsset1.CategoryId, categoryAssetFromRepo1.CategoryId);
        Assert.Equal(categoryAsset1.Asset!.AssetName, categoryAssetFromRepo1.Asset!.AssetName);
        Assert.Equal(categoryAsset1.Asset!.Comment, categoryAssetFromRepo1.Asset!.Comment);
        Assert.Equal(
            categoryAsset1.Asset!.CategoryAssetsCollection!.First().Id,
            categoryAssetFromRepo1.Asset!.CategoryAssetsCollection!.First().Id
        );
        Assert.Equal(categoryAsset1.Category!.CategoryName, categoryAssetFromRepo1.Category!.CategoryName);
        Assert.Equal(categoryAsset1.Category!.Comment, categoryAssetFromRepo1.Category!.Comment);
        Assert.Equal(
            categoryAsset1.Category!.CategoryAssetsCollection!.First().Id,
            categoryAssetFromRepo1.Category!.CategoryAssetsCollection!.First().Id
        );
        
        // CategoryAsset 2
        Assert.Equal(categoryAsset2.Id, categoryAssetFromRepo2.Id);
        Assert.Equal(categoryAsset2.AssetId, categoryAssetFromRepo2.AssetId);
        Assert.Equal(categoryAsset2.CategoryId, categoryAssetFromRepo2.CategoryId);
        Assert.Equal(categoryAsset2.Asset!.AssetName, categoryAssetFromRepo2.Asset!.AssetName);
        Assert.Equal(categoryAsset2.Asset!.Comment, categoryAssetFromRepo2.Asset!.Comment);
        Assert.Equal(
            categoryAsset2.Asset!.CategoryAssetsCollection!.First().Id,
            categoryAssetFromRepo2.Asset!.CategoryAssetsCollection!.First().Id
        );
        Assert.Equal(categoryAsset2.Category!.CategoryName, categoryAssetFromRepo2.Category!.CategoryName);
        Assert.Equal(categoryAsset2.Category!.Comment, categoryAssetFromRepo2.Category!.Comment);
        Assert.Equal(
            categoryAsset2.Category!.CategoryAssetsCollection!.First().Id,
            categoryAssetFromRepo2.Category!.CategoryAssetsCollection!.First().Id
        );
    }

    [Fact]
    public async Task FindAsync_ShouldReturnCategoryAsset()
    {
        // Arrange
        var categoryAsset2 = CreateAssetsAndCategoryAssets()
            .First(ca => ca.Category!.CategoryName == CategoryName2);
        
        // Act
        var categoryAssetFromRepository = await _repository.FindAsync(categoryAsset2.Id);

        // Assert
        Assert.NotNull(categoryAssetFromRepository);
        Assert.Equal(categoryAsset2.Id, categoryAssetFromRepository.Id);
        Assert.Equal(categoryAsset2.AssetId, categoryAssetFromRepository.AssetId);
        Assert.Equal(categoryAsset2.CategoryId, categoryAssetFromRepository.CategoryId);
        Assert.Equal(categoryAsset2.Asset!.AssetName, categoryAssetFromRepository.Asset!.AssetName);
        Assert.Equal(categoryAsset2.Asset!.Comment, categoryAssetFromRepository.Asset!.Comment);
        Assert.Equal(categoryAsset2.Category!.CategoryName, categoryAssetFromRepository.Category!.CategoryName);
        Assert.Equal(categoryAsset2.Category!.Comment, categoryAssetFromRepository.Category!.Comment);
    }

    [Fact]
    public async Task GetCategoryAssetsByAssetId_ShouldReturnCategoryAssetById()
    {
        // Arrange
        var categoryAsset2 = CreateAssetsAndCategoryAssets()
            .First(ca => ca.Category!.CategoryName == CategoryName2);
        
        // Act
        var categoryAssetFromRepository = await _repository.GetCategoryAssetsByAssetId(
            _context.Assets.First(a => a.AssetName == AssetName2).Id);
        
        // Assert
        Assert.NotNull(categoryAssetFromRepository);
        Assert.Equal(categoryAsset2.Id, categoryAssetFromRepository.Id);
        Assert.Equal(categoryAsset2.AssetId, categoryAssetFromRepository.AssetId);
        Assert.Equal(categoryAsset2.CategoryId, categoryAssetFromRepository.CategoryId);
    }

    [Fact]
    public async Task UpdateCategoryOfAsset_ShouldUpdateCategory()
    {
        // Arrange
        var categoryAsset2 = CreateAssetsAndCategoryAssets()
            .First(ca => ca.Category!.CategoryName == CategoryName2);
        var category1 = _context.Categories
            .Include(c => c.CategoryAssetsCollection)
            .First(c => c.CategoryName == CategoryName1);
        
        Assert.Single(category1.CategoryAssetsCollection!);
        
        // Act
        await _repository.UpdateCategoryOfAsset(categoryAsset2.Id, category1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(category1.Id, categoryAsset2.CategoryId);
        Assert.Equal(category1.CategoryName, categoryAsset2.Category!.CategoryName);
        Assert.Equal(category1.Comment, categoryAsset2.Category!.Comment);
        Assert.Equal(2, category1.CategoryAssetsCollection!.Count);
    }
    
    [Fact]
    public async Task UpdateCategoryOfAssetNoCategoryAsset_ShouldNotUpdate()
    {
        // Arrange
        var category1 = _context.Categories
            .Include(c => c.CategoryAssetsCollection)
            .First(c => c.CategoryName == CategoryName1);
        
        // Act
        await _repository.UpdateCategoryOfAsset(Guid.Empty, category1.Id);
        await _repository.UpdateCategoryOfAsset(Guid.NewGuid(), category1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Empty(category1.CategoryAssetsCollection!);
    }

    [Fact]
    public async Task CreateNewCategoryAsset_ShouldCreateNewCategoryAsset()
    {
        // Arrange
        CreateAssets();
        var category1 = _context.Categories.First(c => c.CategoryName == CategoryName1);
        var asset2 = _context.Assets.First(a => a.AssetName == AssetName2);
        
        // Act
        var newCategoryAsset = await _repository.CreateNewCategoryAsset(asset2.Id, category1.Id);
        Assert.NotNull(newCategoryAsset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var dbNewCategoryAsset = _context.CategoryAssetsCollection
            .Include(ca => ca.Category)
            .Include(ca => ca.Asset)
            .FirstOrDefault(ca => ca.Id == newCategoryAsset.Id);
        
        // Assert
        Assert.Equal(category1.Id, newCategoryAsset.CategoryId);
        Assert.Equal(asset2.Id, newCategoryAsset.AssetId);

        Assert.NotNull(dbNewCategoryAsset);
        Assert.Equal(category1.Id, dbNewCategoryAsset.CategoryId);
        Assert.Equal(asset2.Id, dbNewCategoryAsset.AssetId);
        Assert.Equal(category1.CategoryName, dbNewCategoryAsset.Category!.CategoryName);
        Assert.Equal(category1.Comment, dbNewCategoryAsset.Category!.Comment);
        Assert.Equal(asset2.AssetName, dbNewCategoryAsset.Asset!.AssetName);
        Assert.Equal(asset2.Comment, dbNewCategoryAsset.Asset!.Comment);
    }
    
    [Fact]
    public async Task CreateCategoryAssetWithSameAssetIdExists_ShouldReturnNull()
    {
        // Arrange
        var categoryAsset1 = CreateAssetsAndCategoryAssets().First(ra => ra.Asset!.AssetName == AssetName1);
        
        // Act
        var newCategoryAsset = await _repository.CreateNewCategoryAsset(categoryAsset1.AssetId, categoryAsset1.CategoryId);
        Assert.Null(newCategoryAsset);
    }
    
    private (AppDbContext, CategoryAssetsRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new CategoryAssetsRepository(context);

        return (context, repository);
    }

    private List<Domain.CategoryAssets> CreateAssetsAndCategoryAssets()
    {
        var assets = CreateAssets();

        var asset1 = assets.First(a => a.AssetName == AssetName1);
        var asset2 = assets.First(a => a.AssetName == AssetName2);
        
        var category1 = _context.Categories.First(c => c.CategoryName == CategoryName1);
        var category2 = _context.Categories.First(c => c.CategoryName == CategoryName2);

        var categoryAssets1 = new Domain.CategoryAssets()
        {
            AssetId = asset1.Id,
            Asset = asset1,
            CategoryId = category1.Id,
            Category = category1,
        };
        
        var categoryAssets2 = new Domain.CategoryAssets()
        {
            AssetId = asset2.Id,
            Asset = asset2,
            CategoryId = category2.Id,
            Category = category2,
        };
        
        _context.CategoryAssetsCollection.AddRange(categoryAssets1, categoryAssets2);
        _context.SaveChanges();

        var categoryAssetsList = new List<CategoryAssets>();
        categoryAssetsList.AddRange(categoryAssets1, categoryAssets2);
        
        return categoryAssetsList;
    }

    private List<Domain.Asset> CreateAssets()
    {
        var asset1 = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = AssetName1,
            Comment = AssetComment1,
        };
        
        var asset2 = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = AssetName2,
            Comment = AssetComment2,
        };
        
        _context.Assets.AddRange(asset1, asset2);
        _context.SaveChanges();
        
        var assetsList = new List<Asset>();
        assetsList.AddRange(asset1, asset2);
        
        return assetsList;
    }
}