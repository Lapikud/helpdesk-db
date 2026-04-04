using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DTO.v1;

public class RemovedAssets: IDomainId
{
    public Guid Id { get; set; }
    
    [Display(Name = nameof(Asset),
        ResourceType = typeof(App.Resources.Domain.RemovedAssets))]
    public Guid AssetId { get; set; }
    
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
    
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(RemovedBy),
        ResourceType = typeof(App.Resources.Domain.RemovedAssets))]
    public string RemovedBy { get; set; } = default!;
}