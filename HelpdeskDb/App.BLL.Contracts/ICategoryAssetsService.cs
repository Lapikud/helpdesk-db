using App.DAL.Contracts;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface ICategoryAssetsService : IBaseService<App.BLL.DTO.CategoryAssets>, ICategoryAssetsRepositoryCustom
{
    Task<App.BLL.DTO.CategoryAssets?> GetCategoryAssetsByAssetId(Guid assetId);
    Task<App.BLL.DTO.CategoryAssets?> CreateNewCategoryAsset(Guid assetId, Guid categoryId);
}