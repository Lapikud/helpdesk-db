using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class Category : BaseEntity
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string CategoryName { get; set; } = default!;

    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string Comment { get; set; } = default!;

    public ICollection<CategoryAssets>? CategoryAssetsCollection { get; set; }
}