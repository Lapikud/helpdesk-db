using App.Domain.Identity;

namespace WebApp.Areas.Admin.ViewModels;

public class UserManagementVm
{
    public AppUser User { get; set; } = default!;
    public List<AppRole> AllRoles { get; set; } = [];
    public List<AppRole> UserRoles { get; set; } = []; 
}