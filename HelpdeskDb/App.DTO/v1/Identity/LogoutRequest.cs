using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.Identity;

public class LogoutRequest
{
    [MaxLength(128)]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
                ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string RefreshToken { get; set; } = default!;
}
