using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class LocationAssetsUOWMapper : IMapper<App.DAL.DTO.LocationAssets, App.Domain.LocationAssets>
{
    public LocationAssets? Map(Domain.LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    LocationsAssetsCollection = entity.Asset.LocationsAssetsCollection?
                        .Select(x => new App.DAL.DTO.LocationAssets()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new Location()
                {
                    Id = entity.Location.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsAssetsCollection = entity.Location.LocationsAssetsCollection?
                        .Select(x => new App.DAL.DTO.LocationAssets()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            AssetId = x.AssetId,
                        }).ToList()
                }
        };

        return res;
    }

    public Domain.LocationAssets? Map(LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new Domain.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new Domain.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    LocationsAssetsCollection = entity.Asset.LocationsAssetsCollection?
                        .Select(x => new Domain.LocationAssets()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            AssetId = x.AssetId,
                        }).ToList(),
                },
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new Domain.Location()
                {
                    Id = entity.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsAssetsCollection = entity.Location.LocationsAssetsCollection?
                        .Select(x => new Domain.LocationAssets()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            AssetId = x.AssetId,
                        }).ToList()
                }
        };

        return res;
    }
}