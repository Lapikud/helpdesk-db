using App.DAL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class CategoryAssetsBLLMapper : IMapper<App.BLL.DTO.CategoryAssets, App.DAL.DTO.CategoryAssets>
{
    public App.DAL.DTO.CategoryAssets? Map(App.BLL.DTO.CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.CategoryAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            CategoryId = entity.CategoryId,
        };

        return res;
    }

    public App.BLL.DTO.CategoryAssets? Map(App.DAL.DTO.CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.CategoryAssets()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.BLL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    CategoryAssetsCollection = entity.Asset.CategoryAssetsCollection?
                        .Select(x => new App.BLL.DTO.CategoryAssets()
                        {
                            Id = x.Id,
                            CategoryId = x.CategoryId,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            CategoryId = entity.CategoryId,
            Category = entity.Category == null
                ? null
                : new App.BLL.DTO.Category()
                {
                    Id = entity.Category!.Id,
                    CategoryName = entity.Category!.CategoryName,
                    Comment = entity.Category!.Comment,
                    CategoryAssetsCollection = entity.Category.CategoryAssetsCollection?
                        .Select(x => new App.BLL.DTO.CategoryAssets()
                        {
                            Id = x.Id,
                            CategoryId = x.CategoryId,
                            AssetId = x.AssetId,
                        }).ToList()
                }
        };

        return res;
    }
}