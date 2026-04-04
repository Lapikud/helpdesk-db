using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class LocationAssetsBLLMapper: IMapper<App.BLL.DTO.LocationAssets, App.DAL.DTO.LocationAssets>
{
    public App.DAL.DTO.LocationAssets? Map(App.BLL.DTO.LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            LocationId = entity.LocationId,
        };

        return res;
    }

    public App.BLL.DTO.LocationAssets? Map(App.DAL.DTO.LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.BLL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    LocationsAssetsCollection = entity.Asset.LocationsAssetsCollection?
                        .Select(x => new App.BLL.DTO.LocationAssets()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new App.BLL.DTO.Location()
                {
                    Id = entity.Location.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsAssetsCollection = entity.Location.LocationsAssetsCollection?
                        .Select(x => new App.BLL.DTO.LocationAssets()
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