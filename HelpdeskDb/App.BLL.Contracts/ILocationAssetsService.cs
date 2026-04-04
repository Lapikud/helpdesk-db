using App.DAL.Contracts;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface ILocationAssetsService : IBaseService<App.BLL.DTO.LocationAssets>, ILocationAssetsRepositoryCustom
{
    Task<App.BLL.DTO.LocationAssets?> GetLocationAssetsByAssetId(Guid assetId);
    Task<App.BLL.DTO.LocationAssets?> CreateNewLocationAsset(Guid assetId, Guid locationId);
}