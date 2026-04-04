using App.DTO.v1.Identity;
using Base.Contracts;

namespace App.DTO.v1.Mappers.Identity;

public class AppUserRoleMapper : IIdentityMapper<App.DTO.v1.Identity.AppUserRole, App.Domain.Identity.AppUserRole>
{
    public AppUserRole? Map(Domain.Identity.AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId
        };
        return res;
    }

    public Domain.Identity.AppUserRole? Map(AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Identity.AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId
        };
        return res;
    }
    
    public Domain.Identity.AppUserRole Map(App.DTO.v1.CreateObjects.Identity.AppUserRoleCreate entity)
    {
        var res = new Domain.Identity.AppUserRole()
        {
            Id = Guid.NewGuid(),
            UserId = entity.UserId,
            RoleId = entity.RoleId
        };
        return res;
    }
}