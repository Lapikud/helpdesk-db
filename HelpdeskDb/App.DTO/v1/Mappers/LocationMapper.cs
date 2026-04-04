using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class LocationMapper : IMapper<App.DTO.v1.Location, App.BLL.DTO.Location>
{
    public App.DTO.v1.Location? Map(BLL.DTO.Location? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
        };
        return res;
    }

    public BLL.DTO.Location? Map(App.DTO.v1.Location? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
        };
        return res;
    }
    
    public BLL.DTO.Location Map(App.DTO.v1.CreateObjects.LocationCreate entity)
    {
        var res = new App.BLL.DTO.Location()
        {
            Id = Guid.NewGuid(),
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
        };
        return res;
    }
}