using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.BLL.DTO;

public class Location : IDomainId
{
    public Guid Id { get; set; }

    [MaxLength(128,
        ErrorMessageResourceName = nameof(ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(ValidationErrors))]
    [Display(Name = nameof(LocationName),
        Prompt = nameof(App.Resources.Domain.Location.LocationNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Location))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string LocationName { get; set; } = default!;


    [Display(Name = nameof(ShelfNum),
        ResourceType = typeof(App.Resources.Domain.Location))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public int ShelfNum { get; set; }


    [Display(Name = nameof(Column),
        ResourceType = typeof(App.Resources.Domain.Location))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public int Column { get; set; }

    public ICollection<LocationInCupboard>? LocationsInCupboards { get; set; }
    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
}