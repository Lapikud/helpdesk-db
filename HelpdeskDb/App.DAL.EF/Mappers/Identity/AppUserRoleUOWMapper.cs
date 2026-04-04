using App.DAL.DTO.Identity;
using Base.Contracts;

namespace App.DAL.EF.Mappers.Identity;

public class AppUserRoleUOWMapper: IMapper<App.DAL.DTO.Identity.AppUserRole, App.Domain.Identity.AppUserRole>
{
    public App.DAL.DTO.Identity.AppUserRole? Map(Domain.Identity.AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Identity.AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
            User = entity.User == null
                ? null
                : new App.DAL.DTO.Identity.AppUser()
                {
                    Id = entity.User.Id,
                    Username = entity.User.Username!,
                },
            Role = entity.Role == null
                ? null
                : new App.DAL.DTO.Identity.AppRole()
                {
                    Id = entity.Role.Id,
                    Name = entity.Role.Name!,
                },
        };

        return res;
    }

    public Domain.Identity.AppUserRole? Map(AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new App.Domain.Identity.AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
            User = entity.User == null
                ? null
                : new App.Domain.Identity.AppUser()
                {
                    Id = entity.User.Id,
                    Username = entity.User.Username,
                },
            Role = entity.Role == null
                ? null
                : new App.Domain.Identity.AppRole()
                {
                    Id = entity.Role.Id,
                    Name = entity.Role.Name,
                },
        };

        return res;
    }
}