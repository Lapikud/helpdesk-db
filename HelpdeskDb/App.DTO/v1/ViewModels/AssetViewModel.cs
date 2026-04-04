using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.v1.ViewModels;

public class AssetViewModel : IDomainId
{
    public Guid Id { get; set; } // asset id

    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(AssetName),
        Prompt = nameof(App.Resources.Domain.Asset.AssetNamePrompt),
        ResourceType = typeof(App.Resources.Domain.Asset))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string AssetName { get; set; } = default!;

    [Display(Name = nameof(SerialNumber),
        Prompt = nameof(App.Resources.ViewModel.AssetViewModel.SerialNumber),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string? SerialNumber { get; set; }

    [Display(Name = nameof(Barcode),
        Prompt = nameof(App.Resources.ViewModel.AssetViewModel.Barcode),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string? Barcode { get; set; }

    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(CategoryName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string CategoryName { get; set; } = default!;
    
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(OwnerName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string OwnerName { get; set; } = default!;
    
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(RoomName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string RoomName { get; set; } = default!;
    
    [MaxLength(128,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MaxLengthValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [MinLength(2,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.MinLenghtValidationError),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(CupboardName),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public string CupboardName { get; set; } = default!;
    
    
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Display(Name = nameof(ShelfNum),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public int ShelfNum { get; set; }
    
    [Display(Name = nameof(Column),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    [Required(ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.Required),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    [Range(1, 5,
        ErrorMessageResourceName = nameof(Base.Resources.Errors.ValidationErrors.ValueBetween),
        ErrorMessageResourceType = typeof(Base.Resources.Errors.ValidationErrors))]
    public int Column { get; set; }
    
    [Display(Name = nameof(ClosestReservationBy),
        ResourceType = typeof(App.Resources.ViewModel.AssetViewModel))]
    public string ClosestReservationBy { get; set; } = "-";
    
    [Display(Name = nameof(AddedAt),
        ResourceType = typeof(Base.Resources.Common))]
    public DateTime AddedAt { get; set; }
    
    public bool Reserved { get; set; } = false;

    public Guid? ReservationId { get; set; }

    public DateTime? ReservationTo { get; set; }
}