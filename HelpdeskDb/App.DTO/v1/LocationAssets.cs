using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.v1;

public class LocationAssets : IDomainId
{
    public Guid Id { get; set; }
    
    [Display(Name = nameof(Asset),
        ResourceType = typeof(App.Resources.Domain.LocationAssets))]
    public Guid AssetId { get; set; }
    
    [Display(Name = nameof(Location),
        ResourceType = typeof(App.Resources.Domain.LocationAssets))]
    public Guid LocationId { get; set; }
    
    [Display(Name = nameof(CreatedBy),
        ResourceType = typeof(Base.Resources.Common))]
    public string CreatedBy { get; set; } = default!;
}