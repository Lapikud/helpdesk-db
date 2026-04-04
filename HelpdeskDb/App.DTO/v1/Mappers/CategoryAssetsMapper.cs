using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class CategoryAssetsMapper: IMapper<App.DTO.v1.CategoryAssets, App.BLL.DTO.CategoryAssets>
{
    public App.DTO.v1.CategoryAssets? Map(BLL.DTO.CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.CategoryAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            CategoryId = entity.CategoryId,
        };

        return res;
    }

    public BLL.DTO.CategoryAssets? Map(App.DTO.v1.CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.CategoryAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            CategoryId = entity.CategoryId,
        };

        return res;
    }
    
    public App.BLL.DTO.CategoryAssets Map(App.DTO.v1.CreateObjects.CategoryAssetsCreate entity)
    {
        var res = new App.BLL.DTO.CategoryAssets()
        {
            Id = Guid.NewGuid(),
            Comment = entity.Comment,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            CategoryId = entity.CategoryId,
        };
        return res;
    }
}