namespace App.DTO.v1.CreateObjects;

public class AssetReservationCreate
{
    public Guid AssetId { get; set; }
    
    public Guid UserId { get; set; }
    
    public DateTime ReservationFrom { get; set; }
    
    public DateTime ReservationTo { get; set; }
}