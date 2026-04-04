using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class LocationInCupboardMapper : IMapper<App.DTO.v1.LocationInCupboard, App.BLL.DTO.LocationInCupboard>
{
    public App.DTO.v1.LocationInCupboard? Map(BLL.DTO.LocationInCupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            CupboardId = entity.CupboardId,
        };

        return res;
    }

    public BLL.DTO.LocationInCupboard? Map(App.DTO.v1.LocationInCupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            CupboardId = entity.CupboardId,
        };

        return res;
    }
    
    public BLL.DTO.LocationInCupboard Map(App.DTO.v1.CreateObjects.LocationInCupboardCreate entity)
    {
        var res = new App.BLL.DTO.LocationInCupboard()
        {
            Id = Guid.NewGuid(),
            LocationId = entity.LocationId,
            CupboardId = entity.CupboardId,
        };

        return res;
    }
}