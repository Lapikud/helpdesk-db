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
    
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [StringLength(128,
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = "StringLengthBetween",
        MinimumLength = 2)]
    [RegularExpression("^[A-ZÄÖÜÕŠŽ][a-zäöüõšž]+(([- ][A-ZÄÖÜÕŠŽ][a-zäöüõšž]+)*)?$",
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.FirstnameRegex))]
    [Display(Name = nameof(FirstName), ResourceType = typeof(App.Resources.Domain.Identity.AppUser))]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [StringLength(128,
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = "StringLengthBetween",
        MinimumLength = 2)]
    [RegularExpression("^[A-ZÄÖÜÕŠŽ][a-zäöüõšž]+$",
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.LastnameRegex))]
    [Display(Name = nameof(LastName), ResourceType = typeof(App.Resources.Domain.Identity.AppUser))]
    public string LastName { get; set; } = default!;
    
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [EmailAddress(ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.InvalidEmail))]
    [Display(Name = nameof(Email), ResourceType = typeof(Base.Resources.Common))]
    public string Email { get; set; } = default!;
    
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [StringLength(100,
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors),
        ErrorMessageResourceName = "StringLengthBetween",
        MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Password), ResourceType = typeof(Base.Resources.Common))]
    public string Password { get; set; } = default!;
}