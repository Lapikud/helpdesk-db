using System.ComponentModel.DataAnnotations;
using Base.Resources.Errors;

namespace App.DTO.v1.CreateObjects;

public class CategoryAssetsCreate
{
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

    public Guid CategoryId { get; set; }

    public Guid AssetId { get; set; }

    [Display(Name = nameof(CreatedBy),
        ResourceType = typeof(Base.Resources.Common))]
    public string CreatedBy { get; set; } = default!;
}