using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.UpdateObjects;

public class AssetViewModelRemove
{
    public Guid AssetId { get; set; }
    
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