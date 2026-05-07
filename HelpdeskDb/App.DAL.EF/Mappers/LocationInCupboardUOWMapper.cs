using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class LocationInCupboardUOWMapper : IMapper<App.DAL.DTO.LocationInCupboard, App.Domain.LocationInCupboard>
{
    public LocationInCupboard? Map(Domain.LocationInCupboard? entity)
    {
        if (entity == null) return null;
        
        var res = new App.DAL.DTO.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new Location()
                {
                    Id = entity.Location.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsInCupboards = entity.Location.LocationsInCupboards?
                        .Select(x => new LocationInCupboard()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            CupboardId = x.CupboardId,
                        }).ToList()
                },
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    LocationsInCupboards = entity.Cupboard.LocationsInCupboards?
                        .Select(x => new LocationInCupboard()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            CupboardId = x.CupboardId,
                        }).ToList()
                },
        };
        return res;
    }

    public Domain.LocationInCupboard? Map(LocationInCupboard? entity)
    {
        if (entity == null) return null;
        var res = new Domain.LocationInCupboard()
        {
            Id = entity.Id,
            LocationId = entity.LocationId,
            Location = entity.Location == null
                ? null
                : new Domain.Location()
                {
                    Id = entity.Id,
                    LocationName = entity.Location.LocationName,
                    ShelfNum = entity.Location.ShelfNum,
                    Column = entity.Location.Column,
                    LocationsInCupboards = entity.Location.LocationsInCupboards?
                        .Select(x => new Domain.LocationInCupboard()
                        {
                            Id = x.Id,
                            LocationId = x.LocationId,
                            CupboardId = x.CupboardId,
                        }).ToList()
                },
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new Domain.Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    LocationsInCupboards = entity.Cupboard.LocationsInCupboards?
                        .Select(x => new Domain.LocationInCupboard()
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