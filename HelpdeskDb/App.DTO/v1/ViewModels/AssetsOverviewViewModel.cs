namespace App.DTO.v1.ViewModels;

public class AssetsOverviewViewModel
{
    public List<AssetViewModel> AvailableAssets { get; set; } = [];
    public List<AssetViewModel> AssetsReservedByUser { get; set; } = [];
}