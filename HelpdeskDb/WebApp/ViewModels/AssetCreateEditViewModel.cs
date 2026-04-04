using System.ComponentModel.DataAnnotations;
using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class AssetCreateEditViewModel
{
    public Asset Asset { get; set; } = default!;
    
    [Display(Name = nameof(App.Resources.Domain.Category.CategorySingular),
        ResourceType = typeof(App.Resources.Domain.Category))]
    public Guid SelectedCategoryId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Owner.OwnerSingular),
        ResourceType = typeof(App.Resources.Domain.Owner))]
    public Guid SelectedOwnerId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Location.LocationSingular),
        ResourceType = typeof(App.Resources.Domain.Location))]
    public Guid SelectedLocationId { get; set; }

    [ValidateNever]
    public SelectList Categories { get; set; } = default!;
    
    [ValidateNever]
    public SelectList Owners { get; set; } = default!;
    
    [ValidateNever]
    public SelectList Locations { get; set; } = default!;
}