using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class OwnerMapper : IMapper<App.DTO.v1.Owner, App.BLL.DTO.Owner>
{
    public App.DTO.v1.Owner? Map(BLL.DTO.Owner? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.Owner()
        {
            Id = entity.Id,
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
        };
        return res;
    }

    public BLL.DTO.Owner? Map(App.DTO.v1.Owner? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Owner()
        {
            Id = entity.Id,
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
        };
        return res;
    }
    
    public BLL.DTO.Owner Map(App.DTO.v1.CreateObjects.OwnerCreate entity)
    {
        var res = new App.BLL.DTO.Owner()
        {
            Id = Guid.NewGuid(),
            OwnerName = entity.OwnerName,
            Comment = entity.Comment,
        };
        return res;
    }
}