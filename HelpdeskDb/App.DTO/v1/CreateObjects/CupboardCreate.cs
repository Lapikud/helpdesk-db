using System.ComponentModel.DataAnnotations;
using Base.Resources.Errors;

namespace App.DTO.v1.CreateObjects;

public class CupboardCreate
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(CodeName),
        Prompt = nameof(App.Resources.Domain.Cupboard.CodeNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Cupboard))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string CodeName { get; set; } = default!;
}