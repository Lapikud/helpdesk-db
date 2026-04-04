using App.BLL.DTO.ViewModels;
using App.DAL.Contracts;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface IAssetService: IBaseService<App.BLL.DTO.Asset>
{
    Task<App.BLL.DTO.Asset> CreateNewAsset(string assetName, string comment, string? serialNumber = null, string? barcode = null);
    Task<List<AssetViewModel>> GetAvailableAssets();
    Task<List<AssetViewModel>> GetAssetsReservedByUser(Guid userId);
    Task<App.BLL.DTO.ViewModels.AssetViewModel?> GetAssetVmByAssetId(Guid assetId);
    Task<List<App.BLL.DTO.Asset>> GetAllNotRemovedAssets();
}