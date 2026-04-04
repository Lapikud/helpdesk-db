using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class OwnerAssetCreateEditVm
{
    public OwnerAssets OwnerAsset { get; set; } = default!;

    [ValidateNever]
    public SelectList OwnerSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList AssetSelectList { get; set; } = default!;
}