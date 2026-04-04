using App.DTO.v1.Identity;
using Base.Contracts;

namespace App.DTO.v1.Mappers.Identity;

public class AppUserMapper : IIdentityMapper<App.DTO.v1.Identity.AppUser, App.Domain.Identity.AppUser>
{
    public AppUser? Map(Domain.Identity.AppUser? entity)
    {
        if (entity == null) return null;
        var res = new AppUser()
        {
            Id = entity.Id,
            Username = entity!.Username,
        };
        return res;
    }

    public Domain.Identity.AppUser? Map(AppUser? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Identity.AppUser()
        {
            Id = entity.Id,
            Username = entity.Username,
        };
        return res;
    }
    
    public Domain.Identity.AppUser Map(App.DTO.v1.CreateObjects.Identity.AppUserCreate entity)
    {
        var res = new Domain.Identity.AppUser()
        {
            Id = Guid.NewGuid(),
            Username = entity.Username,
        };
        return res;
    }
}