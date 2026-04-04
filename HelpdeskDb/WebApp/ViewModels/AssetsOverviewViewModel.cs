

using App.BLL.DTO.ViewModels;

namespace WebApp.ViewModels;

public class AssetsOverviewViewModel
{
    public List<AssetViewModel> AvailableAssets { get; set; } = [];
    public List<AssetViewModel> AssetsReservedByUser { get; set; } = [];
}