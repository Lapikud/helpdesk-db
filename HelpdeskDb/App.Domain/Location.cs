using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class Location : BaseEntity
{
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string LocationName { get; set; } = default!;


    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public int ShelfNum { get; set; }


    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public int Column { get; set; }

    public ICollection<LocationInCupboard>? LocationsInCupboards { get; set; }
    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
}