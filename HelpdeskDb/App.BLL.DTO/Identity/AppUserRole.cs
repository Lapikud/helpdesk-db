using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.BLL.DTO.Identity;

public class AppUserRole : IDomainId
{
    public Guid Id { get; set; }

    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppUser),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public Guid UserId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppUser),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public AppUser? User { get; set; } = default!;

    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppRole),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public Guid RoleId { get; set; }
    
    [Display(Name = nameof(App.Resources.Domain.Identity.AppUserRole.AppRole),
        ResourceType = typeof(App.Resources.Domain.Identity.AppUserRole))]
    public AppRole? Role { get; set; } = default!;
}