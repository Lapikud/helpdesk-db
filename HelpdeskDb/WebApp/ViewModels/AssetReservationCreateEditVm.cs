using System.ComponentModel.DataAnnotations;
using App.BLL.DTO;
using App.BLL.DTO.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class AssetReservationCreateEditVm
{
    public AssetReservation AssetReservation { get; set; } = default!;

    [Display(Name = nameof(App.Resources.Domain.AssetReservation.Asset),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid SelectedAssetId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.AssetReservation.User),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid SelectedUserId { get; set; }
    
    [ValidateNever]
    public SelectList AssetSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList UserSelectList { get; set; } = default!;
}