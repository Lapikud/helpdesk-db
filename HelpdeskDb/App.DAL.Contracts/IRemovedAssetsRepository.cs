using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface IRemovedAssetsRepository : IBaseRepository<App.DAL.DTO.RemovedAssets>
{
    Task<App.DAL.DTO.RemovedAssets?> GetRemovedAssetByAssetId(Guid assetId);
    Task<App.DAL.DTO.RemovedAssets?> CreateNewRemovedAsset(Guid assetId, string comment);
}