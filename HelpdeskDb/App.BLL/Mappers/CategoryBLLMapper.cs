using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class CategoryBLLMapper : IMapper<App.BLL.DTO.Category, App.DAL.DTO.Category>
{
    public App.DAL.DTO.Category? Map(App.BLL.DTO.Category? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new App.DAL.DTO.CategoryAssets()
            {
                Id = ca.Id,
                Comment = ca.Comment,
                AssetId = ca.AssetId,
                Asset = null, // la.Asset,
                
                CategoryId = ca.CategoryId,
                Category = null,
                
                CreatedBy = ca.CreatedBy,
            }).ToList(),
        };
        return res;
    }

    public App.BLL.DTO.Category? Map(App.DAL.DTO.Category? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new App.BLL.DTO.CategoryAssets()
            {
                Id = ca.Id,
                Comment = ca.Comment,
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