using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.CreateObjects;

public class AssetViewModelCreate
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(AssetName),
        Prompt = nameof(App.Resources.Domain.Asset.AssetNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string AssetName { get; set; } = default!;

    [MaxLength(255,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
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

    [Display(Name = nameof(Category),
        ResourceType = typeof(App.Resources.Domain.CategoryAssets))]
    public Guid SelectedCategoryId { get; set; }
    
    [Display(Name = nameof(Owner),
        ResourceType = typeof(App.Resources.Domain.OwnerAssets))]
    public Guid SelectedOwnerId { get; set; }
    
    [Display(Name = nameof(Location),
        ResourceType = typeof(App.Resources.Domain.LocationAssets))]
    public Guid SelectedLocationId { get; set; }
}