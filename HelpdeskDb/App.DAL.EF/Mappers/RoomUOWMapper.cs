using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class RoomUOWMapper : IMapper<App.DAL.DTO.Room, App.Domain.Room>
{
    public App.DAL.DTO.Room? Map(Domain.Room? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Room()
        {
            Id = entity.Id,
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new App.DAL.DTO.CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
        };
        return res;
    }

    public Domain.Room? Map(App.DAL.DTO.Room? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Room()
        {
            Id = entity.Id,
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new Domain.CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
        };
        return res;
    }
}