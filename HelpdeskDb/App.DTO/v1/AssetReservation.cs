using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.v1;

public class AssetReservation: IDomainId
{
    public Guid Id { get; set; }

    [Display(Name = nameof(App.Resources.Domain.AssetReservation.User),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid UserId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.AssetReservation.Asset),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid AssetId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.AssetReservation.ReservationFrom),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationFrom { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.AssetReservation.ReservationTo),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationTo { get; set; }

    [Display(Name = nameof(App.Resources.Domain.AssetReservation.IsReturned),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public bool IsReturned { get; set; }
}