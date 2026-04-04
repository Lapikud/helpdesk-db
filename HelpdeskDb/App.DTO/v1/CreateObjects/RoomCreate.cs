using System.ComponentModel.DataAnnotations;
using Base.Resources.Errors;

namespace App.DTO.v1.CreateObjects;

public class RoomCreate
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(RoomName),
        Prompt = nameof(App.Resources.Domain.Room.RoomNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Room))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string RoomName { get; set; } = default!;
    
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
}