using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class OwnerUOWMapper : IMapper<App.DAL.DTO.Owner, App.Domain.Owner>
{
    public App.DAL.DTO.Owner? Map(Domain.Owner? entity)
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

    public Domain.Owner? Map(App.DAL.DTO.Owner? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Owner()
        {
            Id = entity.Id,
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
            OwnerAssets = entity.OwnerAssets?.Select(oa => new Domain.OwnerAssets()
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