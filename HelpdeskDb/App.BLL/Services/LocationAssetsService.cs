using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.BLL.Services;

public class LocationAssetsService : BaseService<App.BLL.DTO.LocationAssets, App.DAL.DTO.LocationAssets,
    App.DAL.Contracts.ILocationAssetsRepository>, ILocationAssetsService
{
    public LocationAssetsService(
        IAppUOW serviceUOW,
        IMapper<LocationAssets, DAL.DTO.LocationAssets> mapper) : base(serviceUOW, serviceUOW.LocationAssetsRepository,
        mapper)
    {
    }

    public virtual async Task<LocationAssets?> GetLocationAssetsByAssetId(Guid assetId)
    {
        var locationAssets = await ServiceRepository.GetLocationAssetsByAssetId(assetId);
        return Mapper.Map(locationAssets);
    }

    public virtual async Task<App.BLL.DTO.LocationAssets?> CreateNewLocationAsset(Guid assetId, Guid locationId)
    {
        var locationAsset = await ServiceRepository.CreateNewLocationAsset(assetId, locationId);
        return Mapper.Map(locationAsset)!;
    }

    public virtual async Task UpdateLocationOfAsset(Guid? locationAssetId, Guid locationId, Guid assetId = default)
    {
        await ServiceRepository.UpdateLocationOfAsset(locationAssetId, locationId, assetId);
    }

}