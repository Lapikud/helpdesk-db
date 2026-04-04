using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class CategoryUOWMapper : IMapper<App.DAL.DTO.Category, App.Domain.Category>
{
    public Category? Map(Domain.Category? entity)
    {
        if (entity == null) return null;
        var res = new Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new CategoryAssets()
            {
                Id = ca.Id,
                AssetId = ca.AssetId,
                Asset = null, // la.Asset,
                
                CategoryId = ca.CategoryId,
                Category = null,
                
                CreatedBy = ca.CreatedBy,
            }).ToList(),
        };
        return res;
    }

    public Domain.Category? Map(Category? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new Domain.CategoryAssets()
            {
                Id = ca.Id,
                AssetId = ca.AssetId,
                Asset = null, // la.Asset,
                
                CategoryId = ca.CategoryId,
                Category = null,
                
                CreatedBy = ca.CreatedBy,
            }).ToList(),
        };
        return res;
    }
}