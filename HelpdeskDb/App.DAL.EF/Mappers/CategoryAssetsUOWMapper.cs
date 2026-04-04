using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class CategoryAssetsUOWMapper : IMapper<App.DAL.DTO.CategoryAssets, App.Domain.CategoryAssets>
{
    public CategoryAssets? Map(Domain.CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.CategoryAssets()
        {
            Id = entity.Id,
            CreatedBy = entity.CreatedBy,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    CategoryAssetsCollection = entity.Asset.CategoryAssetsCollection?
                        .Select(x => new App.DAL.DTO.CategoryAssets()
                        {
                            Id = x.Id,
                            CategoryId = x.CategoryId,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            CategoryId = entity.CategoryId,
            Category = entity.Category == null
                ? null
                : new Category()
                {
                    Id = entity.Category!.Id,
                    CategoryName = entity.Category!.CategoryName,
                    Comment = entity.Category!.Comment,
                    CategoryAssetsCollection = entity.Category.CategoryAssetsCollection?
                        .Select(x => new App.DAL.DTO.CategoryAssets()
                        {
                            Id = x.Id,
                            CategoryId = x.CategoryId,
                            AssetId = x.AssetId,
                        }).ToList()
                }
        };

        return res;
    }

    public Domain.CategoryAssets? Map(CategoryAssets? entity)
    {
        if (entity == null) return null;
        var res = new Domain.CategoryAssets()
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
                    CategoryAssetsCollection = entity.Asset.CategoryAssetsCollection?
                        .Select(x => new Domain.CategoryAssets()
                        {
                            Id = x.Id,
                            CategoryId = x.CategoryId,
                            AssetId = x.AssetId,
                        }).ToList()
                },
            CategoryId = entity.CategoryId,
            Category = entity.Category == null
                ? null
                : new Domain.Category()
                {
                    Id = entity.Category!.Id,
                    CategoryName = entity.Category!.CategoryName,
                    Comment = entity.Category!.Comment,
                    CategoryAssetsCollection = entity.Category.CategoryAssetsCollection?
                        .Select(x => new Domain.CategoryAssets()
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