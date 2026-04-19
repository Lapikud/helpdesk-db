using App.DTO.v1.Identity;
using Base.Contracts;

namespace App.DTO.v1.Mappers.Identity;

public class AppRoleMapper : IIdentityMapper<App.DTO.v1.Identity.AppRole, App.Domain.Identity.AppRole>
{
    public AppRole? Map(Domain.Identity.AppRole? entity)
    {
        if (entity == null) return null;

        var res = new AppRole()
        {
            Id = entity.Id,
            Name = entity.Name!
        };
        return res;
    }

    public Domain.Identity.AppRole? Map(AppRole? entity)
    {
        if (entity == null) return null;

        var res = new Domain.Identity.AppRole()
        {
            Id = entity.Id,
            Name = entity.Name,
        };
        return res;
    }
    
    public Domain.Identity.AppRole Map(App.DTO.v1.CreateObjects.Identity.AppRoleCreate entity)
    {

        var res = new Domain.Identity.AppRole()
        {
            Id = Guid.NewGuid(),
            Name = entity.Name,
        };
        return res;
    }
}