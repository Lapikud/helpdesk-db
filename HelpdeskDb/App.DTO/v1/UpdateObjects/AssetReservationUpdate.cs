namespace App.DTO.v1.UpdateObjects;

public class AssetReservationUpdate
{
    public Guid AssetReservationId { get; set; }
    
    public Guid AssetId { get; set; }
    
    public Guid UserId { get; set; }
    
    public DateTime ReservationFrom { get; set; }

    public DateTime ReservationTo { get; set; }

    public bool IsReturned { get; set; }
}