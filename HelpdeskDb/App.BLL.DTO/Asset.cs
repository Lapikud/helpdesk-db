using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.BLL.DTO;

public class Asset: IDomainId
{
    public Guid Id { get; set; }
    
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(AssetName),
        Prompt = nameof(App.Resources.Domain.Asset.AssetNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string AssetName { get; set; } = default!;

    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(Comment),
        Prompt = nameof(Base.Resources.Common.CommentPrompt),   
        ResourceType = typeof(Base.Resources.Common))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string Comment { get; set; } = default!;

    [Display(Name = nameof(App.Resources.Domain.Asset.SerialNumber),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    public string? SerialNumber { get; set; }

    [Display(Name = nameof(App.Resources.Domain.Asset.Barcode),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    public string? Barcode { get; set; }

    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
    public ICollection<OwnerAssets>? OwnerAssets { get; set; }
    public ICollection<CategoryAssets>? CategoryAssetsCollection { get; set; }
    public ICollection<RemovedAssets>? RemovedAssetsCollection { get; set; }
    public ICollection<AssetReservation>? AssetReservations { get; set; }
}
