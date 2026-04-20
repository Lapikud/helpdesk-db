using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class LocationAssetsUOWMapperTest
{
    private readonly LocationAssetsUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.LocationAssets?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.LocationAssets?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new Domain.LocationAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Location);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var src = new Domain.LocationAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId, CreatedBy = "user",
            Asset = new Domain.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                LocationsAssetsCollection = new List<Domain.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId }
                }
            },
            Location = new Domain.Location
            {
                Id = locationId, LocationName = "Location", ShelfNum = 1, Column = 1,
                LocationsAssetsCollection = new List<Domain.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.LocationsAssetsCollection!);
        Assert.NotNull(dst.Location);
        Assert.Single(dst.Location!.LocationsAssetsCollection!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.LocationAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Location);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var src = new DalDto.LocationAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId, CreatedBy = "user",
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment",
                LocationsAssetsCollection = new List<DalDto.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId }
                }
            },
            Location = new DalDto.Location
            {
                Id = locationId, LocationName = "Location", ShelfNum = 1, Column = 1,
                LocationsAssetsCollection = new List<DalDto.LocationAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, LocationId = locationId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Single(dst.Asset!.LocationsAssetsCollection!);
        Assert.NotNull(dst.Location);
        Assert.Single(dst.Location!.LocationsAssetsCollection!);
    }
}
