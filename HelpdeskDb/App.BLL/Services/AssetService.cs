using App.BLL.Contracts;
using App.DAL.Contracts;
using Base.BLL;
using Base.BLL.Contracts;
using Base.Contracts;
using Base.DAL.Contracts;
using App.BLL.DTO;
using App.BLL.DTO.ViewModels;

namespace App.BLL.Services;

public class AssetService : BaseService<App.BLL.DTO.Asset, App.DAL.DTO.Asset, App.DAL.Contracts.IAssetRepository>,
    IAssetService
{
    private readonly App.BLL.Mappers.AssetViewModelMapper _mapper =
        new App.BLL.Mappers.AssetViewModelMapper();
    
    public AssetService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.Asset, App.DAL.DTO.Asset> mapper) : base(serviceUOW, serviceUOW.AssetRepository, mapper)
    {
    }

    public virtual async Task<App.BLL.DTO.Asset> CreateNewAsset(string assetName, string comment, string? serialNumber = null, string? barcode = null)
    {
        var asset = await ServiceRepository.CreateNewAsset(assetName, comment, serialNumber, barcode);
        return Mapper.Map(asset)!;
    }
    
    public virtual async Task<List<App.BLL.DTO.ViewModels.AssetViewModel>> GetAvailableAssets()
    {
        var notTakenAssets = await ServiceRepository.GetAvailableAssets();
        var mapped = notTakenAssets.Select(x => _mapper.Map(x)!).ToList();
        return mapped;
    }

    public virtual async Task<List<App.BLL.DTO.ViewModels.AssetViewModel>> GetAssetsReservedByUser(Guid userId)
    {
        var takenAssetsByUser = await ServiceRepository.GetAssetsReservedByUser(userId);
        var mapped = takenAssetsByUser.Select(x => _mapper.Map(x)!).ToList();
        return mapped;
    }

    public async Task<AssetViewModel?> GetAssetVmByAssetId(Guid assetId)
    {
        var assetVm = await ServiceRepository.GetAssetVmByAssetId(assetId);
        var mapped = _mapper.Map(assetVm)!;
        return mapped;
    }

    public async Task<List<Asset>> GetAllNotRemovedAssets()
    {
        var notRemoved = await ServiceRepository.GetAllNotRemovedAssets();
        var mapped = notRemoved.Select(x => Mapper.Map(x)!).ToList();
        return mapped;
    }
}