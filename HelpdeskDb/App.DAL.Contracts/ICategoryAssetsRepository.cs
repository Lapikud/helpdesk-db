using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface ICategoryAssetsRepository : IBaseRepository<App.DAL.DTO.CategoryAssets>, ICategoryAssetsRepositoryCustom
{
    Task<App.DAL.DTO.CategoryAssets?> GetCategoryAssetsByAssetId(Guid assetId);
    Task<App.DAL.DTO.CategoryAssets?> CreateNewCategoryAsset(Guid assetId, Guid categoryId);
}

public interface ICategoryAssetsRepositoryCustom
{
    Task UpdateCategoryOfAsset(Guid? categoryAssetsId, Guid categoryId, Guid assetId = default);
}