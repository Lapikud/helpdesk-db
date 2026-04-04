using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class Cupboard : BaseEntity
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    public string CodeName { get; set; } = default!;

    public ICollection<LocationInCupboard>? LocationsInCupboards { get; set; }
    public ICollection<CupboardInRoom>? CupboardsInRooms { get; set; }
}