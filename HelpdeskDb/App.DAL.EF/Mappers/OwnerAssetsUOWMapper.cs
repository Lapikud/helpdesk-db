using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class OwnerAssetsUOWMapper : IMapper<App.DAL.DTO.OwnerAssets, App.Domain.OwnerAssets>
{
    public App.DAL.DTO.OwnerAssets? Map(Domain.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.OwnerAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.DAL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    OwnerAssets = entity.Asset.OwnerAssets?
                        .Select(x => new DAL.DTO.OwnerAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            OwnerId = x.OwnerId,
                        }).ToList()
                },
            OwnerId = entity.OwnerId,
            Owner = entity.Owner == null
                ? null
                : new App.DAL.DTO.Owner()
                {
                    Id = entity.Owner!.Id,
                    OwnerName = entity.Owner!.OwnerName,
                    Comment = entity.Owner!.Comment,
                    OwnerAssets = entity.Owner.OwnerAssets?
                        .Select(x => new DAL.DTO.OwnerAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            OwnerId = x.OwnerId,
                        }).ToList()
                }
        };

        return res;
    }

    public Domain.OwnerAssets? Map(App.DAL.DTO.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new Domain.OwnerAssets()
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
                    OwnerAssets = entity.Asset.OwnerAssets?
                        .Select(x => new Domain.OwnerAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            OwnerId = x.OwnerId,
                        }).ToList()
                },
            OwnerId = entity.OwnerId,
            Owner = entity.Owner == null
                ? null
                : new Domain.Owner()
                {
                    Id = entity.Owner!.Id,
                    OwnerName = entity.Owner!.OwnerName,
                    Comment = entity.Owner!.Comment,
                    OwnerAssets = entity.Owner.OwnerAssets?
                        .Select(x => new Domain.OwnerAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            OwnerId = x.OwnerId,
                        }).ToList()
                }
        };

        return res;
    }
}