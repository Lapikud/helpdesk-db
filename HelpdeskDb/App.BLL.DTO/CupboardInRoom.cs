using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.BLL.DTO;

public class CupboardInRoom : IDomainId
{
    public Guid Id { get; set; }

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

    [Display(Name = nameof(Cupboard),
        ResourceType = typeof(App.Resources.Domain.CupboardInRoom))]
    public Guid CupboardId { get; set; }

    [Display(Name = nameof(Cupboard),
        ResourceType = typeof(App.Resources.Domain.CupboardInRoom))]
    public Cupboard? Cupboard { get; set; }

    
    [Display(Name = nameof(Room),
        ResourceType = typeof(App.Resources.Domain.CupboardInRoom))]
    public Guid RoomId { get; set; }
    
    [Display(Name = nameof(Room),
        ResourceType = typeof(App.Resources.Domain.CupboardInRoom))]
    public Room? Room { get; set; }
}