using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class RemovedAssetsUOWMapper : IMapper<App.DAL.DTO.RemovedAssets, App.Domain.RemovedAssets>
{
    public App.DAL.DTO.RemovedAssets? Map(Domain.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.DAL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    RemovedAssetsCollection = entity.Asset.RemovedAssetsCollection?
                        .Select(x => new DAL.DTO.RemovedAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            RemovedBy = entity.CreatedBy
        };

        return res;
    }

    public Domain.RemovedAssets? Map(App.DAL.DTO.RemovedAssets? entity)
    {
        if (entity == null) return null;
        var res = new Domain.RemovedAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new Domain.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    RemovedAssetsCollection = entity.Asset.RemovedAssetsCollection?
                        .Select(x => new Domain.RemovedAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            CreatedBy = entity.RemovedBy
        };

        return res;
    }
}