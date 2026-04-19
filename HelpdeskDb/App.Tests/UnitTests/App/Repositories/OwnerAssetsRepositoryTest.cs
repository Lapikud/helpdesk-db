using App.DAL.EF;
using App.DAL.EF.Repositories;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class OwnerAssetsRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly OwnerAssetsRepository _repository;
    private const string OwnerName1 = "TestOwner1";
    private const string OwnerName2 = "TestOwner2";
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";
    
    public OwnerAssetsRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }
    
    [Fact]
    public async Task AllAsync_ShouldReturnAllOwnerAssets()
    {
        // Arrange
        var ownerAssets = CreateAssetsAndOwnerAssets();
        var ownerAssets1 = ownerAssets.First(oa => oa.Owner!.OwnerName == OwnerName1);
        var ownerAssets2 = ownerAssets.First(oa => oa.Owner!.OwnerName == OwnerName2);

        // Act
        var ownerAssetsFromRepository = (await _repository.AllAsync()).ToList();

        // Assert
        Assert.NotEmpty(ownerAssetsFromRepository);
        Assert.Equal(2, ownerAssetsFromRepository.Count);
        
        var ownerAssetFromRepo1 = ownerAssetsFromRepository
            .First(oa => oa.OwnerId == ownerAssets1.OwnerId &&
                         oa.AssetId == ownerAssets1.AssetId);
        var ownerAssetFromRepo2 = ownerAssetsFromRepository
            .First(oa => oa.OwnerId == ownerAssets2.OwnerId
                         && oa.AssetId == ownerAssets2.AssetId);
        
        // OwnerAsset 1
        Assert.Equal(ownerAssets1.Id, ownerAssetFromRepo1.Id);
        Assert.Equal(ownerAssets1.AssetId, ownerAssetFromRepo1.AssetId);
        Assert.Equal(ownerAssets1.OwnerId, ownerAssetFromRepo1.OwnerId);
        Assert.Equal(ownerAssets1.Asset!.AssetName, ownerAssetFromRepo1.Asset!.AssetName);
        Assert.Equal(ownerAssets1.Asset!.Comment, ownerAssetFromRepo1.Asset!.Comment);
        Assert.Equal(
            ownerAssets1.Asset!.OwnerAssets!.First().Id,
            ownerAssetFromRepo1.Asset!.OwnerAssets!.First().Id
        );
        Assert.Equal(ownerAssets1.Owner!.OwnerName, ownerAssetFromRepo1.Owner!.OwnerName);
        Assert.Equal(ownerAssets1.Owner!.Comment, ownerAssetFromRepo1.Owner!.Comment);
        Assert.Equal(
            ownerAssets1.Owner!.OwnerAssets!.First().Id,
            ownerAssetFromRepo1.Owner!.OwnerAssets!.First().Id
        );
        
        // OwnerAsset 2
        Assert.Equal(ownerAssets2.Id, ownerAssetFromRepo2.Id);
        Assert.Equal(ownerAssets2.AssetId, ownerAssetFromRepo2.AssetId);
        Assert.Equal(ownerAssets2.OwnerId, ownerAssetFromRepo2.OwnerId);
        Assert.Equal(ownerAssets2.Asset!.AssetName, ownerAssetFromRepo2.Asset!.AssetName);
        Assert.Equal(ownerAssets2.Asset!.Comment, ownerAssetFromRepo2.Asset!.Comment);
        Assert.Equal(
            ownerAssets2.Asset!.OwnerAssets!.First().Id,
            ownerAssets2.Asset!.OwnerAssets!.First().Id
        );
        Assert.Equal(ownerAssets2.Owner!.OwnerName, ownerAssetFromRepo2.Owner!.OwnerName);
        Assert.Equal(ownerAssets2.Owner!.Comment, ownerAssetFromRepo2.Owner!.Comment);
        Assert.Equal(
            ownerAssets2.Owner!.OwnerAssets!.First().Id,
            ownerAssetFromRepo2.Owner!.OwnerAssets!.First().Id
        );
    }

    [Fact]
    public async Task FindAsync_ShouldReturnOwnerAsset()
    {
        // Arrange
        var ownerAsset2 = CreateAssetsAndOwnerAssets()
            .First(oa => oa.Owner!.OwnerName == OwnerName2);
        
        // Act
        var ownerAssetFromRepository = await _repository.FindAsync(ownerAsset2.Id);

        // Assert
        Assert.NotNull(ownerAssetFromRepository);
        Assert.Equal(ownerAsset2.Id, ownerAssetFromRepository.Id);
        Assert.Equal(ownerAsset2.AssetId, ownerAssetFromRepository.AssetId);
        Assert.Equal(ownerAsset2.OwnerId, ownerAssetFromRepository.OwnerId);
        Assert.Equal(ownerAsset2.Asset!.AssetName, ownerAssetFromRepository.Asset!.AssetName);
        Assert.Equal(ownerAsset2.Asset!.Comment, ownerAssetFromRepository.Asset!.Comment);
        Assert.Equal(ownerAsset2.Owner!.OwnerName, ownerAssetFromRepository.Owner!.OwnerName);
        Assert.Equal(ownerAsset2.Owner!.Comment, ownerAssetFromRepository.Owner!.Comment);
    }

    [Fact]
    public async Task GetOwnerAssetsByAssetId_ShouldReturnOwnerAssetById()
    {
        // Arrange
        var ownerAsset2 = CreateAssetsAndOwnerAssets()
            .First(oa => oa.Owner!.OwnerName == OwnerName2);
        
        // Act
        var ownerAssetFromRepository = await _repository.GetOwnerAssetsByAssetId(
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
        await _repository.UpdateOwnerOfAsset(ownerAsset2.Id, owner1.Id);
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
        await _repository.UpdateOwnerOfAsset(Guid.Empty, owner1.Id);
        await _repository.UpdateOwnerOfAsset(Guid.NewGuid(), owner1.Id);
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
        var newOwnerAsset = await _repository.CreateNewOwnerAsset(asset2.Id, owner1.Id);
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
        var newOwnerAsset = await _repository.CreateNewOwnerAsset(ownerAsset1.AssetId, ownerAsset1.OwnerId);
        Assert.Null(newOwnerAsset);
    }
    
    private (AppDbContext, OwnerAssetsRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new OwnerAssetsRepository(context);

        return (context, repository);
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