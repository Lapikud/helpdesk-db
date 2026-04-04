using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class LocationAssetCreateEditViewModel
{
    public LocationAssets LocationAssets { get; set; } = default!;

    [ValidateNever]
    public SelectList AssetSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList LocationSelectList { get; set; } = default!;
}