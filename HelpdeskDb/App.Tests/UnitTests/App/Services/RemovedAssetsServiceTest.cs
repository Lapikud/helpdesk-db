using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Services;

public class RemovedAssetsServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly RemovedAssetsService _service;
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";
    private const string RemovedAssetComment1 = "TestRemovedComment1";
    private const string RemovedAssetComment2 = "TestRemovedComment2";
    private const string AssetComment1 = "TestAssetComment1";
    private const string AssetComment2 = "TestAssetComment2";

    public RemovedAssetsServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }
    
    [Fact]
    public async Task GetRemovedAssetsByAssetId_ShouldReturnRemovedAssetById()
    {
        // Arrange
        var removedAsset2 = CreateAssetsAndRemovedAssets()
            .First(ra => ra.Asset!.AssetName == AssetName2);
        
        // Act
        var removedAssetFromRepository = await _service.GetRemovedAssetByAssetId(
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
        var newRemovedAsset = await _service.CreateNewRemovedAsset(asset2.Id, "CreateRemovedAssetTest");
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
        var newRemovedAsset = await _service.CreateNewRemovedAsset(removedAsset1.AssetId, "CreateRemovedAssetTest");
        Assert.Null(newRemovedAsset);
    }
    
    private (AppDbContext, RemovedAssetsService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new RemovedAssetsService(uow, new RemovedAssetsBLLMapper());

        return (context, service);
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