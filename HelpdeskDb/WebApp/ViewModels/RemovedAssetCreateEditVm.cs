using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class RemovedAssetCreateEditVm
{
    public RemovedAssets RemovedAsset { get; set; } = default!;

    [ValidateNever]
    public SelectList AssetSelectList { get; set; } = default!;
}