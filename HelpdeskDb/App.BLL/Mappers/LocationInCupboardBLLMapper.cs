using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class LocationInCupboardBLLMapper: IMapper<App.BLL.DTO.LocationInCupboard, App.DAL.DTO.LocationInCupboard>
{
    public App.DAL.DTO.LocationInCupboard? Map(App.BLL.DTO.LocationInCupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            CupboardId = entity.CupboardId,
        };

        return res;
    }

    public App.BLL.DTO.LocationInCupboard? Map(App.DAL.DTO.LocationInCupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new App.BLL.DTO.Location()
                {
                    Id = entity.Location.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsInCupboards = entity.Location.LocationsInCupboards?
                        .Select(x => new App.BLL.DTO.LocationInCupboard()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            CupboardId = x.CupboardId,
                        }).ToList()
                },
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new App.BLL.DTO.Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    LocationsInCupboards = entity.Cupboard.LocationsInCupboards?
                        .Select(x => new App.BLL.DTO.LocationInCupboard()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            CupboardId = x.CupboardId,
                        }).ToList()
                },
        };

        return res;
    }
}