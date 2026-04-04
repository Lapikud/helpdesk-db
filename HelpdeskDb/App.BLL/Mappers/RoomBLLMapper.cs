using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class RoomBLLMapper: IMapper<App.BLL.DTO.Room, App.DAL.DTO.Room>
{
    public App.DAL.DTO.Room? Map(App.BLL.DTO.Room? entity)
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

    public App.BLL.DTO.Room? Map(App.DAL.DTO.Room? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Room()
        {
            Id = entity.Id,
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new App.BLL.DTO.CupboardInRoom()
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