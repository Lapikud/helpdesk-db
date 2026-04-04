using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class CupboardInRoom : BaseEntity
{
    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string Comment { get; set; } = default!;

    public Guid CupboardId { get; set; }

    public Cupboard? Cupboard { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }
}