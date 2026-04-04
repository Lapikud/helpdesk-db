namespace App.DTO.v1.CreateObjects;

public class LocationInCupboardCreate
{
    public Guid LocationId { get; set; }
    
    public Guid CupboardId { get; set; }
}