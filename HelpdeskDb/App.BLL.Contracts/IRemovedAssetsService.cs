using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface IRemovedAssetsService : IBaseService<App.BLL.DTO.RemovedAssets>
{
    Task<App.BLL.DTO.RemovedAssets?> GetRemovedAssetByAssetId(Guid assetId);
    Task<App.BLL.DTO.RemovedAssets?> CreateNewRemovedAsset(Guid assetId, string comment);
}