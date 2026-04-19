using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.App.Services;

public class LocationAssetsServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly LocationAssetsService _service;
    private const string LocationName1 = "TestLocation1";
    private const string LocationName2 = "TestLocation2";
    private const string AssetName1 = "TestAsset1";
    private const string AssetName2 = "TestAsset2";

    public LocationAssetsServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }
    
    [Fact]
    public async Task GetLocationAssetsByAssetId_ShouldReturnLocationAssetById()
    {
        // Arrange
        var locationAsset2 = CreateAssetsAndLocationAssets()
            .First(la => la.Location!.LocationName == LocationName2);
        
        // Act
        var locationAssetFromRepository = await _service.GetLocationAssetsByAssetId(
            _context.Assets.First(a => a.AssetName == AssetName2).Id);
        
        // Assert
        Assert.NotNull(locationAssetFromRepository);
        Assert.Equal(locationAsset2.Id, locationAssetFromRepository.Id);
        Assert.Equal(locationAsset2.AssetId, locationAssetFromRepository.AssetId);
        Assert.Equal(locationAsset2.LocationId, locationAssetFromRepository.LocationId);
    }

    [Fact]
    public async Task UpdateLocationOfAsset_ShouldUpdateLocation()
    {
        // Arrange
        var locationAsset2 = CreateAssetsAndLocationAssets()
            .First(la => la.Location!.LocationName == LocationName2);
        var location1 = _context.Locations
            .Include(l => l.LocationsAssetsCollection)
            .First(l => l.LocationName == LocationName1);
        
        Assert.Single(location1.LocationsAssetsCollection!);
        
        // Act
        await _service.UpdateLocationOfAsset(locationAsset2.Id, location1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(location1.Id, locationAsset2.LocationId);
        Assert.Equal(location1.LocationName, locationAsset2.Location!.LocationName);
        Assert.Equal(2, location1.LocationsAssetsCollection!.Count);
    }
    
    [Fact]
    public async Task UpdateLocationOfAssetNoLocationAsset_ShouldNotUpdate()
    {
        // Arrange
        var location1 = _context.Locations
            .Include(l => l.LocationsAssetsCollection)
            .First(l => l.LocationName == LocationName1);
        
        // Act
        await _service.UpdateLocationOfAsset(Guid.Empty, location1.Id);
        await _service.UpdateLocationOfAsset(Guid.NewGuid(), location1.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Empty(location1.LocationsAssetsCollection!);
    }

    [Fact]
    public async Task CreateNewLocationAsset_ShouldCreateNewLocationAsset()
    {
        // Arrange
        CreateAssets();
        var location1 = _context.Locations.First(l => l.LocationName == LocationName1);
        var asset2 = _context.Assets.First(a => a.AssetName == AssetName2);
        
        // Act
        var newLocationAsset = await _service.CreateNewLocationAsset(asset2.Id, location1.Id);
        Assert.NotNull(newLocationAsset);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        var dbNewLocationAsset = _context.LocationAssetsCollection
            .Include(la => la.Location)
            .Include(la => la.Asset)
            .FirstOrDefault(la => la.Id == newLocationAsset.Id);
        
        // Assert
        Assert.Equal(location1.Id, newLocationAsset.LocationId);
        Assert.Equal(asset2.Id, newLocationAsset.AssetId);

        Assert.NotNull(dbNewLocationAsset);
        Assert.Equal(location1.Id, dbNewLocationAsset.LocationId);
        Assert.Equal(asset2.Id, dbNewLocationAsset.AssetId);
        Assert.Equal(location1.LocationName, dbNewLocationAsset.Location!.LocationName);
        Assert.Equal(location1.ShelfNum, dbNewLocationAsset.Location!.ShelfNum);
        Assert.Equal(location1.Column, dbNewLocationAsset.Location!.Column);
        Assert.Equal(asset2.AssetName, dbNewLocationAsset.Asset!.AssetName);
        Assert.Equal(asset2.Comment, dbNewLocationAsset.Asset!.Comment);
    }
    
    [Fact]
    public async Task CreateNewLocationAssetWithSameAssetIdExists_ShouldReturnNull()
    {
        // Arrange
        var locationAsset1 = CreateAssetsAndLocationAssets().First(ra => ra.Asset!.AssetName == AssetName1);
        
        // Act
        var newLocationAsset = await _service.CreateNewLocationAsset(locationAsset1.AssetId, locationAsset1.LocationId);
        Assert.Null(newLocationAsset);
    }
    
    private (AppDbContext, LocationAssetsService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new LocationAssetsService(uow, new LocationAssetsBLLMapper());

        return (context, service);
    }
    
    private List<Domain.LocationAssets> CreateAssetsAndLocationAssets()
    {
        var assets = CreateAssets();

        var asset1 = assets.First(a => a.AssetName == AssetName1);
        var asset2 = assets.First(a => a.AssetName == AssetName2);
        
        var location1 = _context.Locations.First(l => l.LocationName == LocationName1);
        var location2 = _context.Locations.First(l => l.LocationName == LocationName2);

        var locationAssets1 = new Domain.LocationAssets()
        {
            AssetId = asset1.Id,
            Asset = asset1,
            LocationId = location1.Id,
            Location = location1,
        };
        
        var locationAssets2 = new Domain.LocationAssets()
        {
            AssetId = asset2.Id,
            Asset = asset2,
            LocationId = location2.Id,
            Location = location2,
        };
        
        _context.LocationAssetsCollection.AddRange(locationAssets1, locationAssets2);
        _context.SaveChanges();

        var locationAssetsList = new List<LocationAssets>();
        locationAssetsList.AddRange(locationAssets1, locationAssets2);
        
        return locationAssetsList;
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