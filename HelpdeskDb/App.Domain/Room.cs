using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class Room : BaseEntity
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string RoomName { get; set; } = default!;

    
    [MaxLength(255,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string Comment { get; set; } = default!;

    public ICollection<CupboardInRoom>? CupboardsInRooms { get; set; }

}