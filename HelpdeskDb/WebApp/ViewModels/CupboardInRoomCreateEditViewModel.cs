using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels;

public class CupboardInRoomCreateEditViewModel
{
    public CupboardInRoom CupboardInRoom { get; set; } = default!;

    [ValidateNever]
    public SelectList CupboardSelectList { get; set; } = default!;
    
    [ValidateNever]
    public SelectList RoomSelectList { get; set; } = default!;
}