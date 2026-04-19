using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Services;

public class OwnerAssetsServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly OwnerAssetsService _service;
    private const string OwnerName1 = "TestOwner1";
    private const string OwnerName2 = "TestOwner2";
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";

    public OwnerAssetsServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }
    
    [Fact]
    public async Task GetOwnerAssetsByAssetId_ShouldReturnOwnerAssetById()
    {
        // Arrange
        var ownerAsset2 = CreateAssetsAndOwnerAssets()
            .First(oa => oa.Owner!.OwnerName == OwnerName2);
        
        // Act
        var ownerAssetFromRepository = await _service.GetOwnerAssetsByAssetId(
            _context.Assets.First(a => a.AssetName == AssetName2).Id);
        
        // Assert
        Assert.NotNull(ownerAssetFromRepository);
        Assert.Equal(ownerAsset2.Id, ownerAssetFromRepository.Id);
        Assert.Equal(ownerAsset2.AssetId, ownerAssetFromRepository.AssetId);
        Assert.Equal(ownerAsset2.OwnerId, ownerAssetFromRepository.OwnerId);
    }

    [Fact]
    public async Task UpdateOwnerOfAsset_ShouldUpdateOwner()
    {
        // Arrange
        var ownerAsset2 = CreateAssetsAndOwnerAssets()
            .First(oa => oa.Owner!.OwnerName == OwnerName2);
        var owner1 = _context.Owners
            .Include(o => o.OwnerAssets)
            .First(o => o.OwnerName == OwnerName1);
        
        Assert.Single(owner1.OwnerAssets!);
        
        // Act
        await _service.UpdateOwnerOfAsset(ownerAsset2.Id, owner1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(owner1.Id, ownerAsset2.OwnerId);
        Assert.Equal(owner1.OwnerName, ownerAsset2.Owner!.OwnerName);
        Assert.Equal(2, owner1.OwnerAssets!.Count);
    }
    
    [Fact]
    public async Task UpdateOwnerOfAssetNoOwnerAsset_ShouldNotUpdate()
    {
        // Arrange
        var owner1 = _context.Owners
            .Include(o => o.OwnerAssets)
            .First(o => o.OwnerName == OwnerName1);
        
        // Act
        await _service.UpdateOwnerOfAsset(Guid.Empty, owner1.Id);
        await _service.UpdateOwnerOfAsset(Guid.NewGuid(), owner1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Empty(owner1.OwnerAssets!);
    }

    [Fact]
    public async Task CreateNewOwnerAsset_ShouldCreateNewOwnerAsset()
    {
        // Arrange
        CreateAssets();
        var owner1 = _context.Owners.First(o => o.OwnerName == OwnerName1);
        var asset2 = _context.Assets.First(a => a.AssetName == AssetName2);
        
        // Act
        var newOwnerAsset = await _service.CreateNewOwnerAsset(asset2.Id, owner1.Id);
        Assert.NotNull(newOwnerAsset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var dbNewOwnerAsset = _context.OwnerAssets
            .Include(oa => oa.Owner)
            .Include(oa => oa.Asset)
            .FirstOrDefault(oa => oa.Id == newOwnerAsset.Id);
        
        // Assert
        Assert.Equal(owner1.Id, newOwnerAsset.OwnerId);
        Assert.Equal(asset2.Id, newOwnerAsset.AssetId);

        Assert.NotNull(dbNewOwnerAsset);
        Assert.Equal(owner1.Id, dbNewOwnerAsset.OwnerId);
        Assert.Equal(asset2.Id, dbNewOwnerAsset.AssetId);
        Assert.Equal(owner1.OwnerName, dbNewOwnerAsset.Owner!.OwnerName);
        Assert.Equal(owner1.Comment, dbNewOwnerAsset.Owner!.Comment);
        Assert.Equal(asset2.AssetName, dbNewOwnerAsset.Asset!.AssetName);
        Assert.Equal(asset2.Comment, dbNewOwnerAsset.Asset!.Comment);
    }
    
    [Fact]
    public async Task CreateNewOwnerAssetWithSameAssetIdExists_ShouldReturnNull()
    {
        // Arrange
        var ownerAsset1 = CreateAssetsAndOwnerAssets().First(ra => ra.Asset!.AssetName == AssetName1);
        
        // Act
        var newOwnerAsset = await _service.CreateNewOwnerAsset(ownerAsset1.AssetId, ownerAsset1.OwnerId);
        Assert.Null(newOwnerAsset);
    }
    
    private (AppDbContext, OwnerAssetsService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new OwnerAssetsService(uow, new OwnerAssetsBLLMapper());

        return (context, service);
    }
    
    private List<Domain.OwnerAssets> CreateAssetsAndOwnerAssets()
    {
        var assets = CreateAssets();

        var asset1 = assets.First(a => a.AssetName == AssetName1);
        var asset2 = assets.First(a => a.AssetName == AssetName2);
        
        var owner1 = _context.Owners.First(o => o.OwnerName == OwnerName1);
        var owner2 = _context.Owners.First(o => o.OwnerName == OwnerName2);

        var ownerAssets1 = new Domain.OwnerAssets()
        {
            AssetId = asset1.Id,
            Asset = asset1,
            OwnerId = owner1.Id,
            Owner = owner1,
        };
        
        var ownerAssets2 = new Domain.OwnerAssets()
        {
            AssetId = asset2.Id,
            Asset = asset2,
            OwnerId = owner2.Id,
            Owner = owner2,
        };
        
        _context.OwnerAssets.AddRange(ownerAssets1, ownerAssets2);
        _context.SaveChanges();

        var ownerAssetsList = new List<OwnerAssets>();
        ownerAssetsList.AddRange(ownerAssets1, ownerAssets2);
        
        return ownerAssetsList;
    }

    private List<Domain.Asset> CreateAssets()
    {
        var asset1 = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = AssetName1,
            Comment = "TestComment1",
        };
        
        var asset2 = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = AssetName2,
            Comment = "TestComment2",
        };
        
        _context.Assets.AddRange(asset1, asset2);
        _context.SaveChanges();
        
        var assetsList = new List<Asset>();
        assetsList.AddRange(asset1, asset2);
        
        return assetsList;
    }
}