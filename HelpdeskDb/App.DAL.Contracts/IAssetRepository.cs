
using App.DAL.DTO.ViewModels;
using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface IAssetRepository: IBaseRepository<App.DAL.DTO.Asset>
{
    Task<App.DAL.DTO.Asset> CreateNewAsset(string assetName, string comment, string? serialNumber = null, string? barcode = null);
    Task<List<AssetViewModel>> GetAvailableAssets();
    Task<List<AssetViewModel>> GetAssetsReservedByUser(Guid userId);
    Task<App.DAL.DTO.ViewModels.AssetViewModel?> GetAssetVmByAssetId(Guid assetId);
    Task<List<App.DAL.DTO.Asset>> GetAllNotRemovedAssets();
}