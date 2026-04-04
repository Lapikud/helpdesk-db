using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DAL.DTO;

public class LocationInCupboard : IDomainId
{    
    public Guid Id { get; set; }

    public Guid LocationId { get; set; }
    
    public Location? Location { get; set; }

    public Guid CupboardId { get; set; }
    
    public Cupboard? Cupboard { get; set; }
}