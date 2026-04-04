using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class RemovedAssetsService : BaseService<App.BLL.DTO.RemovedAssets, App.DAL.DTO.RemovedAssets, App.DAL.Contracts.IRemovedAssetsRepository>,
    IRemovedAssetsService
{
    public RemovedAssetsService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.RemovedAssets, App.DAL.DTO.RemovedAssets> mapper) : base(serviceUOW, serviceUOW.RemovedAssetsRepository, mapper)
    {
    }
    
    public async Task<RemovedAssets?> GetRemovedAssetByAssetId(Guid assetId)
    {
        var removedAsset = await ServiceRepository.GetRemovedAssetByAssetId(assetId);
        return Mapper.Map(removedAsset);
    }

    public async Task<RemovedAssets?> CreateNewRemovedAsset(Guid assetId, string comment)
    {
        var removedAsset = await ServiceRepository.CreateNewRemovedAsset(assetId, comment);
        return Mapper.Map(removedAsset)!;
    }
}