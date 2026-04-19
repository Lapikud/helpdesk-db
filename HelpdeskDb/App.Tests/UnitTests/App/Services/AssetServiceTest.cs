using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Services;

public class AssetServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly AssetService _service;

    public AssetServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }
    
    [Fact]
    public async Task GetAssetVmByAssetId_ShouldReturnVmWithCorrectData()
    {
        // Arrange
        var asset = CreateAssetWithCategoryLocationAndOwner();
        _context.ChangeTracker.Clear();

        // Act
        var assetVm = await _service.GetAssetVmByAssetId(asset.Id);

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
    public async Task GetAvailableAssets_ShouldReturnAllAssetsWithoutRemovedAssets()
    {
        // Arrange
        CreateAssetWithRemovedAssets();
        var assetWithoutReservation = CreateAssetWithCategoryLocationAndOwner();
        
        // Act
        var availableAssets = await _service.GetAvailableAssets();
        var assetWithoutRemovedAssetsVm = await _service.GetAssetVmByAssetId(assetWithoutReservation.Id);
        var assetWithoutRemovedAssetsFromService = await _service.FindAsync(assetWithoutReservation.Id);
        
        // Assert
        Assert.Equal(2, _context.Assets.Count());
        Assert.NotEmpty(availableAssets);
        Assert.Single(availableAssets);
        var notRemovedAssetWithoutReservation = availableAssets.First();
        
        Assert.NotNull(assetWithoutRemovedAssetsFromService);
        
        Assert.Equal(assetWithoutRemovedAssetsFromService.Id, notRemovedAssetWithoutReservation.Id);
        Assert.Equal(assetWithoutRemovedAssetsFromService.AssetName, notRemovedAssetWithoutReservation.AssetName);
        
        Assert.NotNull(assetWithoutRemovedAssetsVm);
        Assert.Equal(assetWithoutRemovedAssetsVm.Id, notRemovedAssetWithoutReservation.Id);
        Assert.Equal(assetWithoutRemovedAssetsVm.AssetName, notRemovedAssetWithoutReservation.AssetName);
        Assert.InRange(assetWithoutRemovedAssetsVm.AddedAt,
            notRemovedAssetWithoutReservation.AddedAt - TimeSpan.FromSeconds(10),
            notRemovedAssetWithoutReservation.AddedAt + TimeSpan.FromSeconds(10)
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.CategoryName,
            notRemovedAssetWithoutReservation.CategoryName
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.OwnerName,
            notRemovedAssetWithoutReservation.OwnerName
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.CupboardName,
            notRemovedAssetWithoutReservation.CupboardName
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.RoomName,
            notRemovedAssetWithoutReservation.RoomName
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.ShelfNum,
            notRemovedAssetWithoutReservation.ShelfNum
        );
        Assert.Equal(
            assetWithoutRemovedAssetsVm.Column,
            notRemovedAssetWithoutReservation.Column
        );
        Assert.Equal(assetWithoutRemovedAssetsVm.SerialNumber, notRemovedAssetWithoutReservation.SerialNumber);
        Assert.Equal(assetWithoutRemovedAssetsVm.Barcode, notRemovedAssetWithoutReservation.Barcode);
        Assert.False(notRemovedAssetWithoutReservation.Reserved);
        Assert.Equal(assetWithoutRemovedAssetsVm.Reserved, notRemovedAssetWithoutReservation.Reserved);
    }
    
    [Fact]
    public async Task GetAssetsReservedByUser_ShouldReturnAllAssetsReservedByUser()
    {
        // Arrange
        var assetWithAssetReservation = CreateAssetWithAssetReservation();
        CreateAssetWithRemovedAssets();
        CreateAssetWithCategoryLocationAndOwner();
        
        // Act
        var assetsReservedByUser = await _service.GetAssetsReservedByUser(_context.Users.First().Id);
        var assetWithAssetReservationVm = await _service.GetAssetVmByAssetId(assetWithAssetReservation.Id);
        var assetWithAssetReservationFromService = await _service.FindAsync(assetWithAssetReservation.Id);

        // Assert
        Assert.Equal(3, _context.Assets.Count());
        Assert.NotEmpty(assetsReservedByUser);
        Assert.Single(assetsReservedByUser);
        var reservedAsset = assetsReservedByUser.First();
        
        Assert.NotNull(assetWithAssetReservationFromService);
        
        Assert.Equal(assetWithAssetReservationFromService.Id, reservedAsset.Id);
        Assert.Equal(assetWithAssetReservationFromService.AssetName, reservedAsset.AssetName);
        
        Assert.NotNull(assetWithAssetReservationVm);
        Assert.Equal(assetWithAssetReservationVm.Id, reservedAsset.Id);
        Assert.Equal(assetWithAssetReservationVm.AssetName, reservedAsset.AssetName);
        Assert.Equal(assetWithAssetReservationVm.ClosestReservationBy, reservedAsset.ClosestReservationBy);
        Assert.InRange(assetWithAssetReservationVm.AddedAt,
            reservedAsset.AddedAt - TimeSpan.FromSeconds(10),
            reservedAsset.AddedAt + TimeSpan.FromSeconds(10)
        );
        Assert.Equal(
            assetWithAssetReservationVm.CategoryName,
            reservedAsset.CategoryName
        );
        Assert.Equal(
            assetWithAssetReservationVm.OwnerName,
            reservedAsset.OwnerName
        );
        Assert.Equal(
            assetWithAssetReservationVm.CupboardName,
            reservedAsset.CupboardName
        );
        Assert.Equal(
            assetWithAssetReservationVm.RoomName,
            reservedAsset.RoomName
        );
        Assert.Equal(
            assetWithAssetReservationVm.ShelfNum,
            reservedAsset.ShelfNum
        );
        Assert.Equal(
            assetWithAssetReservationVm.Column,
            reservedAsset.Column
        );
        Assert.Equal(
            assetWithAssetReservationVm.SerialNumber,
            reservedAsset.SerialNumber
        );
        Assert.Equal(
            assetWithAssetReservationVm.Barcode,
            reservedAsset.Barcode
        );
        Assert.True(assetWithAssetReservationVm.Reserved);
        Assert.Equal(
            assetWithAssetReservationVm.Reserved,
            reservedAsset.Reserved
        );
        Assert.Equal(
            assetWithAssetReservationVm.ReservationTo,
            reservedAsset.ReservationTo
        );
    }
    
    private (AppDbContext, AssetService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new AssetService(uow, new AssetBLLMapper());

        return (context, service);
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
            .Include(l => l.LocationsInCupboards)! // Load LocationInCupboard
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

    private Domain.Asset CreateAssetWithAssetReservation()
    {
        var asset = CreateAssetWithCategoryLocationAndOwner();
        var user = _context.Users.First();

        var userAsset = new Domain.AssetReservation()
        {
            AssetId = asset.Id,
            UserId = user.Id,
            ReservationFrom = DateTime.UtcNow,
            ReservationTo = DateTime.UtcNow.AddHours(1)
        };
        
        asset.AssetReservations = new List<Domain.AssetReservation>();
        asset.AssetReservations.Add(userAsset);

        return asset;
    }
    
    private void CreateAssetWithRemovedAssets()
    {
        var asset = CreateAssetWithCategoryLocationAndOwner();

        var removedAsset = new Domain.RemovedAssets()
        {
            AssetId = asset.Id,
            Comment = "Test Remove"
        };
        
        asset.RemovedAssetsCollection = new List<Domain.RemovedAssets>();
        asset.RemovedAssetsCollection.Add(removedAsset);
    }
}