using App.DAL.EF;
using App.DAL.EF.Repositories;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class RemovedAssetsRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly RemovedAssetsRepository _repository;
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";
    private const string RemovedAssetComment1 = "TestRemovedComment1";
    private const string RemovedAssetComment2 = "TestRemovedComment2";
    private const string AssetComment1 = "TestAssetComment1";
    private const string AssetComment2 = "TestAssetComment2";
    
    public RemovedAssetsRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }
    
    [Fact]
    public async Task AllAsync_ShouldReturnAllRemovedAssets()
    {
        // Arrange
        var removedAssets = CreateAssetsAndRemovedAssets();
        var removedAssets1 = removedAssets.First(ra => ra.Asset!.AssetName == AssetName1);
        var removedAssets2 = removedAssets.First(ra => ra.Asset!.AssetName == AssetName2);

        // Act
        var removedAssetsFromRepository = (await _repository.AllAsync()).ToList();

        // Assert
        Assert.NotEmpty(removedAssetsFromRepository);
        Assert.Equal(2, removedAssetsFromRepository.Count);
        
        var removedAssetFromRepo1 = removedAssetsFromRepository.First(ra => ra.Asset!.AssetName == AssetName1);
        var removedAssetFromRepo2 = removedAssetsFromRepository.First(ra => ra.Asset!.AssetName == AssetName2);
        
        // RemovedAsset 1
        Assert.Equal(removedAssets1.Id, removedAssetFromRepo1.Id);
        Assert.Equal(removedAssets1.AssetId, removedAssetFromRepo1.AssetId);
        Assert.Equal(removedAssets1.Asset!.AssetName, removedAssetFromRepo1.Asset!.AssetName);
        Assert.Equal(removedAssets1.Asset!.Comment, removedAssetFromRepo1.Asset!.Comment);
        Assert.Equal(
            removedAssets1.Asset!.RemovedAssetsCollection!.First().Id,
            removedAssetFromRepo1.Asset!.RemovedAssetsCollection!.First().Id
        );
        Assert.Equal(removedAssets1.Comment, removedAssetFromRepo1.Comment);
        
        // RemovedAsset 2
        Assert.Equal(removedAssets2.Id, removedAssetFromRepo2.Id);
        Assert.Equal(removedAssets2.AssetId, removedAssetFromRepo2.AssetId);
        Assert.Equal(removedAssets2.Asset!.AssetName, removedAssetFromRepo2.Asset!.AssetName);
        Assert.Equal(removedAssets2.Asset!.Comment, removedAssetFromRepo2.Asset!.Comment);
        Assert.Equal(
            removedAssets2.Asset!.RemovedAssetsCollection!.First().Id,
            removedAssets2.Asset!.RemovedAssetsCollection!.First().Id
        );
        Assert.Equal(removedAssets2.Comment, removedAssetFromRepo2.Comment);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnRemovedAsset()
    {
        // Arrange
        var removedAsset2 = CreateAssetsAndRemovedAssets()
            .First(ra => ra.Asset!.AssetName == AssetName2);
        
        // Act
        var removedAssetFromRepository = await _repository.FindAsync(removedAsset2.Id);

        // Assert
        Assert.NotNull(removedAssetFromRepository);
        Assert.Equal(removedAsset2.Id, removedAssetFromRepository.Id);
        Assert.Equal(removedAsset2.AssetId, removedAssetFromRepository.AssetId);
        Assert.Equal(removedAsset2.Asset!.AssetName, removedAssetFromRepository.Asset!.AssetName);
        Assert.Equal(removedAsset2.Asset!.Comment, removedAssetFromRepository.Asset!.Comment);
        Assert.Equal(removedAsset2.Comment, removedAssetFromRepository.Comment);
    }

    [Fact]
    public async Task GetRemovedAssetsByAssetId_ShouldReturnRemovedAssetById()
    {
        // Arrange
        var removedAsset2 = CreateAssetsAndRemovedAssets()
            .First(ra => ra.Asset!.AssetName == AssetName2);
        
        // Act
        var removedAssetFromRepository = await _repository.GetRemovedAssetByAssetId(
            _context.Assets.First(a => a.AssetName == AssetName2).Id);
        
        // Assert
        Assert.NotNull(removedAssetFromRepository);
        Assert.Equal(removedAsset2.Id, removedAssetFromRepository.Id);
        Assert.Equal(removedAsset2.AssetId, removedAssetFromRepository.AssetId);
        Assert.Equal(removedAsset2.Comment, removedAssetFromRepository.Comment);
    }
    
    [Fact]
    public async Task CreateNewRemovedAsset_ShouldCreateNewRemovedAsset()
    {
        // Arrange
        CreateAssets();
        var asset2 = _context.Assets.First(a => a.AssetName == AssetName2);
        
        // Act
        var newRemovedAsset = await _repository.CreateNewRemovedAsset(asset2.Id, "CreateRemovedAssetTest");
        Assert.NotNull(newRemovedAsset);
        
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var dbNewRemovedAsset = _context.RemovedAssetsCollection
            .Include(ra => ra.Asset)
            .FirstOrDefault(ra => ra.Id == newRemovedAsset.Id);
        
        // Assert
        Assert.Equal(asset2.Id, newRemovedAsset.AssetId);

        Assert.NotNull(dbNewRemovedAsset);
        Assert.Equal(asset2.Id, dbNewRemovedAsset.AssetId);
        Assert.Equal(asset2.AssetName, dbNewRemovedAsset.Asset!.AssetName);
        Assert.Equal(asset2.Comment, dbNewRemovedAsset.Asset!.Comment);
        Assert.Equal("CreateRemovedAssetTest", dbNewRemovedAsset.Comment);
    }
    
    [Fact]
    public async Task CreateNewRemovedAssetWithSameAssetIdExists_ShouldReturnNull()
    {
        // Arrange
        var removedAsset1 = CreateAssetsAndRemovedAssets().First(ra => ra.Asset!.AssetName == AssetName1);
        
        // Act
        var newRemovedAsset = await _repository.CreateNewRemovedAsset(removedAsset1.AssetId, "CreateRemovedAssetTest");
        Assert.Null(newRemovedAsset);
    }
    
    private (AppDbContext, RemovedAssetsRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new RemovedAssetsRepository(context);

        return (context, repository);
    }
    
    private List<Domain.RemovedAssets> CreateAssetsAndRemovedAssets()
    {
        var assets = CreateAssets();

        var asset1 = assets.First(a => a.AssetName == AssetName1);
        var asset2 = assets.First(a => a.AssetName == AssetName2);

        var removedAssets1 = new Domain.RemovedAssets()
        {
            AssetId = asset1.Id,
            Asset = asset1,
            Comment = RemovedAssetComment1
        };
        
        var removedAssets2 = new Domain.RemovedAssets()
        {
            AssetId = asset2.Id,
            Asset = asset2,
            Comment = RemovedAssetComment2
        };
        
        _context.RemovedAssetsCollection.AddRange(removedAssets1, removedAssets2);
        _context.SaveChanges();

        var removedAssetsList = new List<RemovedAssets>();
        removedAssetsList.AddRange(removedAssets1, removedAssets2);
        
        return removedAssetsList;
    }

    private List<Domain.Asset> CreateAssets()
    {
        var asset1 = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = AssetName1,
            Comment = AssetComment1
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