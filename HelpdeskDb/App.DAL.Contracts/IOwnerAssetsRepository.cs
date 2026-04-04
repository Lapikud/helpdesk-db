using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface IOwnerAssetsRepository : IBaseRepository<App.DAL.DTO.OwnerAssets>, IOwnerAssetsRepositoryCustom
{
    Task<App.DAL.DTO.OwnerAssets?> GetOwnerAssetsByAssetId(Guid assetId);
    Task< App.DAL.DTO.OwnerAssets?> CreateNewOwnerAsset(Guid assetId, Guid ownerId);
}

public interface IOwnerAssetsRepositoryCustom
{
    Task UpdateOwnerOfAsset(Guid? ownerAssetsId, Guid ownerId, Guid assetId = default);
}