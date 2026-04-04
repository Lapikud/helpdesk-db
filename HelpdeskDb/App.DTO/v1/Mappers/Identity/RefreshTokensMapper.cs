using App.DTO.v1.Identity;
using Base.Contracts;

namespace App.DTO.v1.Mappers.Identity;

public class RefreshTokensMapper : IIdentityMapper<App.DTO.v1.Identity.AppRefreshToken, App.Domain.Identity.AppRefreshToken>
{
    public AppRefreshToken? Map(Domain.Identity.AppRefreshToken? entity)
    {
        if (entity == null) return null;
        var res = new AppRefreshToken()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RefreshToken = entity.RefreshToken,
            PreviousRefreshToken = entity.PreviousRefreshToken,
            Expiration = entity.Expiration,
            PreviousExpiration = entity.PreviousExpiration
        };
        return res;
    }

    public Domain.Identity.AppRefreshToken? Map(AppRefreshToken? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Identity.AppRefreshToken()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RefreshToken = entity.RefreshToken,
            PreviousRefreshToken = entity.PreviousRefreshToken,
            Expiration = entity.Expiration,
            PreviousExpiration = entity.PreviousExpiration
        };
        return res;
    }
    
    // public Domain.Identity.AppRefreshToken Map(App.DTO.v1.CreateObjects.Identity.AppRefreshTokenCreate entity)
    // {
    //     var res = new Domain.Identity.AppRefreshToken()
    //     {
    //         Id = Guid.NewGuid(),
    //     };
    //     return res;
    // }
}