using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.CreateObjects;

public class LocationAssetsCreate
{
    public Guid AssetId { get; set; }
    
    public Guid LocationId { get; set; }
    
    [Display(Name = nameof(CreatedBy),
        ResourceType = typeof(Base.Resources.Common))]
    public string CreatedBy { get; set; } = default!;
}