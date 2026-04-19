using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class LocationAssetsBLLMapperTest
{
    private readonly LocationAssetsBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.LocationAssets?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.LocationAssets?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.LocationAssets
        {
            Id = Guid.NewGuid(), CreatedBy = "u",
            AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid()
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.LocationId, dst.LocationId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedAssetAndLocation_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var locId = Guid.NewGuid();
        var src = new DalDto.LocationAssets
        {
            Id = Guid.NewGuid(), CreatedBy = "u", AssetId = assetId, LocationId = locId,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                LocationsAssetsCollection = new List<DalDto.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locId }
                }
            },
            Location = new DalDto.Location
            {
                Id = locId, LocationName = "L", ShelfNum = 1, Column = 1,
                LocationsAssetsCollection = new List<DalDto.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(assetId, dst!.Asset!.Id);
        Assert.Single(dst.Asset.LocationsAssetsCollection!);
        Assert.Equal(locId, dst.Location!.Id);
        Assert.Single(dst.Location.LocationsAssetsCollection!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.LocationAssets { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Asset);
        Assert.Null(dst.Location);
    }
}
