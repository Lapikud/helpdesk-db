using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class LocationInCupboardCreateEditVm
{
    public LocationInCupboard LocationInCupboard { get; set; } = default!;

    [ValidateNever]
    public SelectList CupboardSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList LocationSelectList { get; set; } = default!;
}