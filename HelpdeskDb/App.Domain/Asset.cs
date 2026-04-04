using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class Asset : BaseEntity
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string AssetName { get; set; } = default!;

    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string Comment { get; set; } = default!;

    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(App.Resources.Domain.Asset.SerialNumber),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    public string? SerialNumber { get; set; }

    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(App.Resources.Domain.Asset.Barcode),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    public string? Barcode { get; set; }

    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
    public ICollection<OwnerAssets>? OwnerAssets { get; set; }
    public ICollection<CategoryAssets>? CategoryAssetsCollection { get; set; }
    public ICollection<RemovedAssets>? RemovedAssetsCollection { get; set; }
    public ICollection<AssetReservation>? AssetReservations { get; set; }
}