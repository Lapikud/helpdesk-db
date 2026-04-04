using App.DAL.EF;
using App.DAL.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Repositories;

[Collection("NonParallel")]
public class AssetRepositoryTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly AssetRepository _repository;

    public AssetRepositoryTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _repository) = SetupDependencies();
    }

    [Fact]
    public async Task GetAssetVmByAssetId_ShouldReturnVmWithCorrectData()
    {
        // Arrange
        var asset = CreateAssetWithCategoryLocationAndOwner();
        _context.ChangeTracker.Clear();

        // Act
        var assetVm = await _repository.GetAssetVmByAssetId(asset.Id);

        // Assert
        Assert.NotNull(assetVm);
        Assert.Equal(assetVm.Id, asset.Id);
        Assert.Equal(assetVm.AssetName, asset.AssetName);
        Assert.InRange(assetVm.AddedAt,
            asset.LocationsAssetsCollection!.First().CreatedAt - TimeSpan.FromSeconds(10),
            asset.LocationsAssetsCollection!.First().CreatedAt + TimeSpan.FromSeconds(10)
        );
        Assert.Equal(
            assetVm.CategoryName,
            asset.CategoryAssetsCollection!.First()
                .Category!.CategoryName
        );
        Assert.Equal(
            assetVm.OwnerName,
            asset.OwnerAssets!.First().Owner!.OwnerName
        );
        Assert.Equal(
            assetVm.CupboardName,
            asset.LocationsAssetsCollection!.First()
                .Location!.LocationsInCupboards!.First()
                .Cupboard!.CodeName
        );
        Assert.Equal(
            assetVm.RoomName,
            asset.LocationsAssetsCollection!.First()
                .Location!.LocationsInCupboards!.First()
                .Cupboard!.CupboardsInRooms!.First()
                .Room!.RoomName
        );
        Assert.Equal(
            assetVm.ShelfNum,
            asset.LocationsAssetsCollection!.First().Location!.ShelfNum
        );
        Assert.Equal(
            assetVm.Column,
            asset.LocationsAssetsCollection!.First().Location!.Column
        );
    }

    [Fact]
    public async Task GetNotTakenAssets_ShouldReturnAllAssetsWithoutUserAssets()
    {
        // Arrange
        CreateAssetWithUserAssets();
        CreateAssetWithRemovedAssets();
        var assetWithoutUserAsset = CreateAssetWithCategoryLocationAndOwner();
        
        // Act
        var notTakenAssets = await _repository.GetAvailableAssets();
        var assetWithoutUserAssetVm = await _repository.GetAssetVmByAssetId(assetWithoutUserAsset.Id);
        var assetWithoutUserAssetFromRepository = await _repository.FindAsync(assetWithoutUserAsset.Id);
        
        // Assert
        Assert.Equal(3, _context.Assets.Count());
        Assert.NotEmpty(notTakenAssets);
        Assert.Single(notTakenAssets);
        var notTakenAsset = notTakenAssets.First();
        
        Assert.NotNull(assetWithoutUserAssetFromRepository);
        
        Assert.Equal(assetWithoutUserAssetFromRepository.Id, notTakenAsset.Id);
        Assert.Equal(assetWithoutUserAssetFromRepository.AssetName, notTakenAsset.AssetName);
        
        Assert.NotNull(assetWithoutUserAssetVm);
        Assert.Equal(assetWithoutUserAssetVm.Id, notTakenAsset.Id);
        Assert.Equal(assetWithoutUserAssetVm.AssetName, notTakenAsset.AssetName);
        Assert.InRange(assetWithoutUserAssetVm.AddedAt,
            notTakenAsset.AddedAt - TimeSpan.FromSeconds(10),
            notTakenAsset.AddedAt + TimeSpan.FromSeconds(10)
        );
        Assert.Equal(
            assetWithoutUserAssetVm.CategoryName,
            notTakenAsset.CategoryName
        );
        Assert.Equal(
            assetWithoutUserAssetVm.OwnerName,
            notTakenAsset.OwnerName
        );
        Assert.Equal(
            assetWithoutUserAssetVm.CupboardName,
            notTakenAsset.CupboardName
        );
        Assert.Equal(
            assetWithoutUserAssetVm.RoomName,
            notTakenAsset.RoomName
        );
        Assert.Equal(
            assetWithoutUserAssetVm.ShelfNum,
            notTakenAsset.ShelfNum
        );
        Assert.Equal(
            assetWithoutUserAssetVm.Column,
            notTakenAsset.Column
        );
    }
    
    [Fact]
    public async Task GetAssetsReservedByUser_ShouldReturnAllAssetsTakenByUser()
    {
        // Arrange
        var assetWithUserAsset = CreateAssetWithUserAssets();
        CreateAssetWithRemovedAssets();
        CreateAssetWithCategoryLocationAndOwner();
        
        // Act
        var takenAssets = await _repository.GetAssetsReservedByUser(_context.Users.First().Id);
        var assetWithUserAssetVm = await _repository.GetAssetVmByAssetId(assetWithUserAsset.Id);
        var assetWithUserAssetFromRepository = await _repository.FindAsync(assetWithUserAsset.Id);

        // Assert
        Assert.Equal(3, _context.Assets.Count());
        Assert.NotEmpty(takenAssets);
        Assert.Single(takenAssets);
        var takenAsset = takenAssets.First();
        
        Assert.NotNull(assetWithUserAssetFromRepository);
        
        Assert.Equal(assetWithUserAssetFromRepository.Id, takenAsset.Id);
        Assert.Equal(assetWithUserAssetFromRepository.AssetName, takenAsset.AssetName);
        
        Assert.NotNull(assetWithUserAssetVm);
        Assert.Equal(assetWithUserAssetVm.Id, takenAsset.Id);
        Assert.Equal(assetWithUserAssetVm.AssetName, takenAsset.AssetName);
        Assert.Equal(assetWithUserAssetVm.ClosestReservationBy, takenAsset.ClosestReservationBy);
        Assert.InRange(assetWithUserAssetVm.AddedAt,
            takenAsset.AddedAt - TimeSpan.FromSeconds(10),
            takenAsset.AddedAt + TimeSpan.FromSeconds(10)
        );
        Assert.Equal(
            assetWithUserAssetVm.CategoryName,
            takenAsset.CategoryName
        );
        Assert.Equal(
            assetWithUserAssetVm.OwnerName,
            takenAsset.OwnerName
        );
        Assert.Equal(
            assetWithUserAssetVm.CupboardName,
            takenAsset.CupboardName
        );
        Assert.Equal(
            assetWithUserAssetVm.RoomName,
            takenAsset.RoomName
        );
        Assert.Equal(
            assetWithUserAssetVm.ShelfNum,
            takenAsset.ShelfNum
        );
        Assert.Equal(
            assetWithUserAssetVm.Column,
            takenAsset.Column
        );
    }
    
    [Fact]
    public async Task AllAsync_ShouldReturnAllAssetsWithCollections()
    {
        // Arrange
        var asset = CreateAssetWithCategoryLocationAndOwner();

        // Act
        var assetsFromRepository = (await _repository.AllAsync()).ToList();

        // Assert
        Assert.NotEmpty(assetsFromRepository);
        var assetFromRepository = assetsFromRepository.First();

        Assert.Equal(asset.AssetName, assetFromRepository.AssetName);
        Assert.Equal(asset.Comment, assetFromRepository.Comment);

        Assert.NotNull(assetFromRepository.CategoryAssetsCollection);
        Assert.NotNull(assetFromRepository.LocationsAssetsCollection);
        Assert.NotNull(assetFromRepository.OwnerAssets);
        Assert.NotNull(assetFromRepository.RemovedAssetsCollection);
        Assert.NotNull(assetFromRepository.AssetReservations);

        Assert.NotEmpty(assetFromRepository.CategoryAssetsCollection);
        Assert.NotEmpty(assetFromRepository.LocationsAssetsCollection);
        Assert.NotEmpty(assetFromRepository.OwnerAssets);
        Assert.Empty(assetFromRepository.RemovedAssetsCollection);
        Assert.Empty(assetFromRepository.AssetReservations);

        Assert.Equal(
            asset.CategoryAssetsCollection!.First().CategoryId,
            assetFromRepository.CategoryAssetsCollection.First().CategoryId
        );

        Assert.Equal(
            asset.LocationsAssetsCollection!.First().LocationId,
            assetFromRepository.LocationsAssetsCollection.First().LocationId
        );

        Assert.Equal(
            asset.OwnerAssets!.First().OwnerId,
            assetFromRepository.OwnerAssets.First().OwnerId
        );
    }

    private (AppDbContext, AssetRepository) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var repository = new AssetRepository(context);
        context.Database.BeginTransaction();

        return (context, repository);
    }

    private Domain.Asset CreateAssetWithCategoryLocationAndOwner()
    {
        var asset = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = "Test Asset",
            Comment = "Test Comment",
        };

        var category = _context.Categories.First();
        var location = _context.Locations
            .Include(l => l.LocationsInCupboards)!// Load LocationInCupboard
            .ThenInclude(lc => lc.Cupboard) // Load Cupboard
            .ThenInclude(c => c!.CupboardsInRooms)! // Load CupboardInRoom
            .ThenInclude(cr => cr.Room) // Load Room
            .First(l => l.LocationName == "TestLocation2");
        var owner = _context.Owners.First();

        var categoryAsset = new Domain.CategoryAssets()
        {
            AssetId = asset.Id,
            CategoryId = category.Id,
            Category = category,
        };
        var locationAsset = new Domain.LocationAssets()
        {
            AssetId = asset.Id,
            LocationId = location.Id,
            Location = location,
        };
        var ownerAsset = new Domain.OwnerAssets()
        {
            AssetId = asset.Id,
            OwnerId = owner.Id,
            Owner = owner,
        };

        _context.Assets.Add(asset);
        _context.CategoryAssetsCollection.Add(categoryAsset);
        _context.LocationAssetsCollection.Add(locationAsset);
        _context.OwnerAssets.Add(ownerAsset);
        _context.SaveChanges();

        asset.CategoryAssetsCollection = new List<Domain.CategoryAssets>();
        asset.CategoryAssetsCollection.Add(categoryAsset);

        asset.LocationsAssetsCollection = new List<Domain.LocationAssets>();
        asset.LocationsAssetsCollection.Add(locationAsset);

        asset.OwnerAssets = new List<Domain.OwnerAssets>();
        asset.OwnerAssets.Add(ownerAsset);
        return asset;
    }

    private Domain.Asset CreateAssetWithUserAssets()
    {
        var asset = CreateAssetWithCategoryLocationAndOwner();
        var user = _context.Users.First();

        // var userAsset = new Domain.UserAssets()
        // {
        //     AssetId = asset.Id,
        //     UserId = user.Id
        // };
        //
        // asset.UserAssetsCollection = new List<Domain.UserAssets>();
        // asset.UserAssetsCollection.Add(userAsset);
        // asset.LastTakenBy = "TestUser";

        return asset;
    }
    
    private Domain.Asset CreateAssetWithRemovedAssets()
    {
        var asset = CreateAssetWithCategoryLocationAndOwner();

        var removedAsset = new Domain.RemovedAssets()
        {
            AssetId = asset.Id,
            Comment = "Test Remove"
        };
        
        asset.RemovedAssetsCollection = new List<Domain.RemovedAssets>();
        asset.RemovedAssetsCollection.Add(removedAsset);

        return asset;
    }
}