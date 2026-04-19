using System.ComponentModel.DataAnnotations;
using Base.Resources.Errors;

namespace App.DTO.v1.CreateObjects.Identity;

public class AppUserCreate
{
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [StringLength(64,
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = "StringLengthBetween",
        MinimumLength = 3)]
    [RegularExpression("^(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$",
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = "UsernameRegex")]
    [Display(Name = nameof(Username), ResourceType = typeof(Base.Resources.Common))]
    public string Username { get; set; } = default!;
    
}