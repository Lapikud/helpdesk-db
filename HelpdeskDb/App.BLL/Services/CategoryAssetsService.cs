using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class CategoryAssetsService : BaseService<App.BLL.DTO.CategoryAssets, App.DAL.DTO.CategoryAssets, App.DAL.Contracts.ICategoryAssetsRepository>,
    ICategoryAssetsService
{
    public CategoryAssetsService(
        IAppUOW serviceUOW,
        IMapper<CategoryAssets, DAL.DTO.CategoryAssets> mapper) : base(serviceUOW, serviceUOW.CategoryAssetsRepository, mapper)
    {
    }

    public virtual async Task<App.BLL.DTO.CategoryAssets?> CreateNewCategoryAsset(Guid assetId, Guid categoryId)
    {
        var categoryAsset = await ServiceRepository.CreateNewCategoryAsset(assetId, categoryId);
        return Mapper.Map(categoryAsset)!;
    }

    public virtual async Task<BLL.DTO.CategoryAssets?> GetCategoryAssetsByAssetId(Guid assetId)
    {
        var categoryAssets = await ServiceRepository.GetCategoryAssetsByAssetId(assetId);
        return Mapper.Map(categoryAssets);
    }

    public virtual async Task UpdateCategoryOfAsset(Guid? categoryAssetsId, Guid categoryId, Guid assetId = default)
    {
        await ServiceRepository.UpdateCategoryOfAsset(categoryAssetsId, categoryId, assetId);
    }
}