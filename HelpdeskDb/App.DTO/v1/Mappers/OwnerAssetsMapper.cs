using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class OwnerAssetsMapper : IMapper<App.DTO.v1.OwnerAssets, App.BLL.DTO.OwnerAssets>
{
    public App.DTO.v1.OwnerAssets? Map(BLL.DTO.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.OwnerAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            OwnerId = entity.OwnerId,
        };

        return res;
    }

    public BLL.DTO.OwnerAssets? Map(App.DTO.v1.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new BLL.DTO.OwnerAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            OwnerId = entity.OwnerId,
        };

        return res;
    }
    
    public BLL.DTO.OwnerAssets Map(App.DTO.v1.CreateObjects.OwnerAssetsCreate entity)
    {
        var res = new BLL.DTO.OwnerAssets()
        {
            Id = Guid.NewGuid(),
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            OwnerId = entity.OwnerId,
        };

        return res;
    }
}