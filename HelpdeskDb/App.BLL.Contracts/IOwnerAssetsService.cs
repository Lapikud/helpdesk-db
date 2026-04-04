using App.DAL.Contracts;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface IOwnerAssetsService : IBaseService<App.BLL.DTO.OwnerAssets>, IOwnerAssetsRepositoryCustom
{
    Task<App.BLL.DTO.OwnerAssets?> GetOwnerAssetsByAssetId(Guid assetId);
    Task<App.BLL.DTO.OwnerAssets?> CreateNewOwnerAsset(Guid assetId, Guid ownerId);
}