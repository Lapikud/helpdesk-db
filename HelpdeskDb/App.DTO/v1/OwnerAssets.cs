using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.v1;

public class OwnerAssets: IDomainId
{
    public Guid Id { get; set; }
    
    [Display(Name = nameof(Owner),
        ResourceType = typeof(App.Resources.Domain.OwnerAssets))]
    public Guid OwnerId { get; set; }
    
    [Display(Name = nameof(Asset),
        ResourceType = typeof(App.Resources.Domain.OwnerAssets))]
    public Guid AssetId { get; set; }
    
    [Display(Name = nameof(CreatedBy),
        ResourceType = typeof(Base.Resources.Common))]
    public string CreatedBy { get; set; } = default!;
}