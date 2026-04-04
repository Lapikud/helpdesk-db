using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.BLL.DTO.ViewModels;

public class AssetViewModel : IDomainId
{
    public Guid Id { get; set; } // asset id

    [Display(Name = nameof(AssetName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string AssetName { get; set; } = default!;

    [Display(Name = nameof(SerialNumber),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string? SerialNumber { get; set; }

    [Display(Name = nameof(Barcode),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string? Barcode { get; set; }

    [Display(Name = nameof(CategoryName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string CategoryName { get; set; } = default!;
    
    [Display(Name = nameof(OwnerName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string OwnerName { get; set; } = default!;
    
    [Display(Name = nameof(RoomName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string RoomName { get; set; } = default!;
    
    [Display(Name = nameof(CupboardName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string CupboardName { get; set; } = default!;
    
    [Display(Name = nameof(ShelfNum),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public int ShelfNum { get; set; }
    
    [Display(Name = nameof(Column),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public int Column { get; set; }
    
    [Display(Name = nameof(ClosestReservationBy),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string ClosestReservationBy { get; set; } = "-";
    
    [Display(Name = nameof(AddedAt),
        ResourceType = typeof(Base.Resources.Common))]
    public DateTime AddedAt { get; set; }
    
    public bool Reserved { get; set; } = false;

    public Guid? ReservationId { get; set; }

    public DateTime? ReservationTo { get; set; }
}