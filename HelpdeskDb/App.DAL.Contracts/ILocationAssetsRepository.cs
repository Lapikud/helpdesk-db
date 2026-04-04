using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface ILocationAssetsRepository : IBaseRepository<App.DAL.DTO.LocationAssets>, ILocationAssetsRepositoryCustom
{
    Task<App.DAL.DTO.LocationAssets?> GetLocationAssetsByAssetId(Guid assetId);
    Task<App.DAL.DTO.LocationAssets?> CreateNewLocationAsset(Guid assetId, Guid locationId);
}

public interface ILocationAssetsRepositoryCustom
{
    Task UpdateLocationOfAsset(Guid? locationAssetId, Guid locationId, Guid assetId = default);
}