using System.ComponentModel.DataAnnotations;

namespace App.DTO.v1.CreateObjects.Identity;

public class AppUserRoleCreate
{
    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppUser),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public Guid UserId { get; set; }
    

    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppRole),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public Guid RoleId { get; set; }
    
}