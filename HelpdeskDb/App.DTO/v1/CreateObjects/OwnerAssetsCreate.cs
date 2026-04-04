using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.CreateObjects;

public class OwnerAssetsCreate
{    
    public Guid OwnerId { get; set; }
    
    public Guid AssetId { get; set; }
    
    [Display(Name = nameof(CreatedBy),
        ResourceType = typeof(Base.Resources.Common))]
    public string CreatedBy { get; set; } = default!;
}