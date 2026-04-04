using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class OwnerAssetsService : BaseService<App.BLL.DTO.OwnerAssets, App.DAL.DTO.OwnerAssets, App.DAL.Contracts.IOwnerAssetsRepository>,
    IOwnerAssetsService
{
    public OwnerAssetsService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.OwnerAssets, App.DAL.DTO.OwnerAssets> mapper) : base(serviceUOW, serviceUOW.OwnerAssetsRepository, mapper)
    {
    }

    public virtual async Task UpdateOwnerOfAsset(Guid? ownerAssetsId, Guid ownerId, Guid assetId = default)
    {
        await ServiceRepository.UpdateOwnerOfAsset(ownerAssetsId, ownerId, assetId);
    }

    public virtual async Task<OwnerAssets?> GetOwnerAssetsByAssetId(Guid assetId)
    {
        var ownerAssets = await ServiceRepository.GetOwnerAssetsByAssetId(assetId);
        return Mapper.Map(ownerAssets);
    }

    public async Task<App.BLL.DTO.OwnerAssets?> CreateNewOwnerAsset(Guid assetId, Guid ownerId)
    {
        var ownerAsset = await ServiceRepository.CreateNewOwnerAsset(assetId, ownerId);
        return Mapper.Map(ownerAsset)!;
    }
}