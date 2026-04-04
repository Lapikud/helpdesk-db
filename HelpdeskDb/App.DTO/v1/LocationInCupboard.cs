using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.v1;

public class LocationInCupboard: IDomainId
{
    public Guid Id { get; set; }
    
    [Display(Name = nameof(Location),
        ResourceType = typeof(App.Resources.Domain.LocationInCupboard))]
    public Guid LocationId { get; set; }
    
    [Display(Name = nameof(Cupboard),
        ResourceType = typeof(App.Resources.Domain.LocationInCupboard))]
    public Guid CupboardId { get; set; }
}