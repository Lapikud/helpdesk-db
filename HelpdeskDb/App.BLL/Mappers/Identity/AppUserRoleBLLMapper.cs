using App.BLL.DTO.Identity;
using Base.Contracts;

namespace App.BLL.Mappers.Identity;

public class AppUserRoleBLLMapper: IMapper<App.BLL.DTO.Identity.AppUserRole, App.DAL.DTO.Identity.AppUserRole>
{
    public App.DAL.DTO.Identity.AppUserRole? Map(App.BLL.DTO.Identity.AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Identity.AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
        };

        return res;
    }

    public App.BLL.DTO.Identity.AppUserRole? Map(App.DAL.DTO.Identity.AppUserRole? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Identity.AppUserRole()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
            User = entity.User == null
                ? null
                : new App.BLL.DTO.Identity.AppUser()
                {
                    Id = entity.User.Id,
                    Username = entity.User.Username!,
                },
            Role = entity.Role == null
                ? null
                : new App.BLL.DTO.Identity.AppRole()
                {
                    Id = entity.Role.Id,
                    Name = entity.Role.Name,
                },
        };

        return res;
    }
}