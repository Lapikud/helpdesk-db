using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class OwnerAssetsBLLMapper: IMapper<App.BLL.DTO.OwnerAssets, App.DAL.DTO.OwnerAssets>
{
    public App.DAL.DTO.OwnerAssets? Map(App.BLL.DTO.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.OwnerAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            OwnerId = entity.OwnerId,
        };

        return res;
    }

    public App.BLL.DTO.OwnerAssets? Map(App.DAL.DTO.OwnerAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.OwnerAssets()
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
                    OwnerAssets = entity.Asset.OwnerAssets?
                        .Select(x => new App.BLL.DTO.OwnerAssets()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            OwnerId = x.OwnerId,
                        }).ToList()
                },
            OwnerId = entity.OwnerId,
            Owner = entity.Owner == null
                ? null
                : new App.BLL.DTO.Owner()
                {
                    Id = entity.Owner!.Id,
                    OwnerName = entity.Owner!.OwnerName,
                    Comment = entity.Owner!.Comment,
                    OwnerAssets = entity.Owner.OwnerAssets?
                        .Select(x => new App.BLL.DTO.OwnerAssets()
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