using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class RemovedAssets : BaseEntity
{
    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }

    
    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string Comment { get; set; } = default!;
}