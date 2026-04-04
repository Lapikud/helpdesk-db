using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class LocationAssetsMapper: IMapper<App.DTO.v1.LocationAssets, App.BLL.DTO.LocationAssets>
{
    public App.DTO.v1.LocationAssets? Map(BLL.DTO.LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            LocationId = entity.LocationId,
        };

        return res;
    }

    public BLL.DTO.LocationAssets? Map(App.DTO.v1.LocationAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.LocationAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            LocationId = entity.LocationId,
        };

        return res;
    }
    
    public BLL.DTO.LocationAssets Map(App.DTO.v1.CreateObjects.LocationAssetsCreate entity)
    {
        var res = new App.BLL.DTO.LocationAssets()
        {
            Id = Guid.NewGuid(),
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            LocationId = entity.LocationId,
        };

        return res;
    }
}