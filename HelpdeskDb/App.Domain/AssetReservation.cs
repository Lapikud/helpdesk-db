using App.Domain.Identity;
using Base.Domain;

namespace App.Domain;

public class AssetReservation: BaseEntityUser<AppUser>
{
    public Guid AssetId { get; set; }
    public Asset? Asset { get; set; }
    public DateTime ReservationFrom { get; set; }
    public DateTime ReservationTo { get; set; }
    
    public bool IsReturned { get; set; }
}