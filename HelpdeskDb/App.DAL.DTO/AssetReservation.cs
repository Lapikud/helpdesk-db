using App.DAL.DTO.Identity;
using Base.Contracts;

namespace App.DAL.DTO;

public class AssetReservation: IDomainId
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    
    public AppUser? User { get; set; }
    
    public Guid AssetId { get; set; }
    
    public Asset? Asset { get; set; }
    public DateTime ReservationFrom { get; set; }
    public DateTime ReservationTo { get; set; }

    public bool IsReturned { get; set; }
}