using System.ComponentModel.DataAnnotations;
using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class AssetRemoveViewModel
{
    public Asset Asset { get; set; } = default!;
    
    [Display(Name = nameof(App.Resources.Domain.Category.CategorySingular),
        ResourceType = typeof(App.Resources.Domain.Category))]
    public Guid SelectedCategoryId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Owner.OwnerSingular),
        ResourceType = typeof(App.Resources.Domain.Owner))]
    public Guid SelectedOwnerId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Location.LocationSingular),
        ResourceType = typeof(App.Resources.Domain.Location))]
    public Guid SelectedLocationId { get; set; }

    [ValidateNever]
    public SelectList Categories { get; set; } = default!;
    
    [ValidateNever]
    public SelectList Owners { get; set; } = default!;
    
    [ValidateNever]
    public SelectList Locations { get; set; } = default!;

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
}