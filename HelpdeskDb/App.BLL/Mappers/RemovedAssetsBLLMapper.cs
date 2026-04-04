using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class RemovedAssetsBLLMapper: IMapper<App.BLL.DTO.RemovedAssets, App.DAL.DTO.RemovedAssets>
{
    public App.DAL.DTO.RemovedAssets? Map(App.BLL.DTO.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            RemovedBy = entity.RemovedBy
        };

        return res;
    }

    public App.BLL.DTO.RemovedAssets? Map(App.DAL.DTO.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.BLL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    RemovedAssetsCollection = entity.Asset.RemovedAssetsCollection?
                        .Select(x => new App.BLL.DTO.RemovedAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            RemovedBy = entity.RemovedBy
        };

        return res;
    }
}