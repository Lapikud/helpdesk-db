using System.ComponentModel.DataAnnotations;
using Base.Resources.Errors;

namespace App.DTO.v1.Identity;

public class AppRefreshToken
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(1,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string RefreshToken { get; set; } = Guid.NewGuid().ToString();
    
    public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);
    
    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(1,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string? PreviousRefreshToken { get; set; }
    
    public DateTime PreviousExpiration { get; set; } = DateTime.UtcNow.AddDays(7);
}