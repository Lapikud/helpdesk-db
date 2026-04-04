using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class RemovedAssetsMapper : IMapper<App.DTO.v1.RemovedAssets, App.BLL.DTO.RemovedAssets>
{
    public App.DTO.v1.RemovedAssets? Map(BLL.DTO.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            RemovedBy = entity.RemovedBy
        };

        return res;
    }

    public BLL.DTO.RemovedAssets? Map(App.DTO.v1.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new BLL.DTO.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            RemovedBy = entity.RemovedBy
        };

        return res;
    }
    
    public BLL.DTO.RemovedAssets Map(App.DTO.v1.CreateObjects.RemovedAssetsCreate entity)
    {
        var res = new BLL.DTO.RemovedAssets()
        {
            Id = Guid.NewGuid(),
            Comment = entity.Comment,
            AssetId = entity.AssetId,
        };

        return res;
    }
}