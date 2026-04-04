using System.ComponentModel.DataAnnotations;
using App.BLL.DTO.Identity;
using Base.Contracts;

namespace App.BLL.DTO;

public class AssetReservation : IDomainId
{
    public Guid Id { get; set; }

    [Display(Name = nameof(User),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid UserId { get; set; }

    [Display(Name = nameof(User),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public AppUser? User { get; set; }

    [Display(Name = nameof(Asset),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Guid AssetId { get; set; }

    [Display(Name = nameof(Asset),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public Asset? Asset { get; set; }

    [Display(Name = nameof(ReservationFrom),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationFrom { get; set; }
    
    [Display(Name = nameof(ReservationTo),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationTo { get; set; }
    
    [Display(Name = nameof(ReservationFrom),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationFromLocal => ReservationFrom.ToLocalTime();
    
    [Display(Name = nameof(ReservationTo),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public DateTime ReservationToLocal => ReservationTo.ToLocalTime();

    [Display(Name = nameof(IsReturned),
        ResourceType = typeof(App.Resources.Domain.AssetReservation))]
    public bool IsReturned { get; set; }
}