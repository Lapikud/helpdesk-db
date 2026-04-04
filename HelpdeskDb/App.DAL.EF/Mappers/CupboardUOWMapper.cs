using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class CupboardUOWMapper : IMapper<App.DAL.DTO.Cupboard, App.Domain.Cupboard>
{
    public Cupboard? Map(Domain.Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
        };
        return res;
    }

    public Domain.Cupboard? Map(Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new Domain.CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new Domain.LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
        };
        return res;
    }
}