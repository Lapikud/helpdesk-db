using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class OwnerBLLMapper: IMapper<App.BLL.DTO.Owner, App.DAL.DTO.Owner>
{
    public App.DAL.DTO.Owner? Map(App.BLL.DTO.Owner? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Owner()
        {
            Id = entity.Id,
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
            OwnerAssets = entity.OwnerAssets?.Select(oa => new App.DAL.DTO.OwnerAssets()
            {
                Id = oa.Id,
                AssetId = oa.AssetId,
                Asset = null, // la.Asset,
                
                OwnerId = oa.OwnerId,
                Owner = null,
                CreatedBy = oa.CreatedBy
            }).ToList(),
        };
        return res;
    }

    public App.BLL.DTO.Owner? Map(App.DAL.DTO.Owner? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Owner()
        {
            Id = entity.Id,
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
            OwnerAssets = entity.OwnerAssets?.Select(oa => new App.BLL.DTO.OwnerAssets()
            {
                Id = oa.Id,
                AssetId = oa.AssetId,
                Asset = null, // la.Asset,
                
                OwnerId = oa.OwnerId,
                Owner = null,
                CreatedBy = oa.CreatedBy
            }).ToList(),
        };
        return res;
    }
}