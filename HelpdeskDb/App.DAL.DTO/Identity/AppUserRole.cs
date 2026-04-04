using Base.Contracts;

namespace App.DAL.DTO.Identity;

public class AppUserRole : IDomainId
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public AppUser? User { get; set; } = default!;
    
    public Guid RoleId { get; set; }
    
    public AppRole? Role { get; set; } = default!;
}