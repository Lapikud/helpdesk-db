using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class CategoryAssetCreateEditViewModel
{
    public CategoryAssets CategoryAssets { get; set; } = default!;

    [ValidateNever]
    public SelectList AssetSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList CategorySelectList { get; set; } = default!;
}