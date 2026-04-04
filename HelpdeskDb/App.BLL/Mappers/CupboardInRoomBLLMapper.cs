using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class CupboardInRoomBLLMapper: IMapper<App.BLL.DTO.CupboardInRoom, App.DAL.DTO.CupboardInRoom>
{
    public App.DAL.DTO.CupboardInRoom? Map(App.BLL.DTO.CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            RoomId = entity.RoomId,
        };

        return res;
    }

    public App.BLL.DTO.CupboardInRoom? Map(App.DAL.DTO.CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new App.BLL.DTO.Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    CupboardsInRooms = entity.Cupboard.CupboardsInRooms?
                        .Select(x => new App.BLL.DTO.CupboardInRoom()
                        {
                            Id = x.Id,
                            CupboardId = x.CupboardId,
                            RoomId = x.RoomId,
                        }).ToList()
                },
            RoomId = entity.RoomId,
            Room = entity.Room == null
                ? null
                : new App.BLL.DTO.Room()
                {
                    Id = entity.Room!.Id,
                    RoomName = entity.Room!.RoomName,
                    Comment = entity.Room!.Comment,
                    CupboardsInRooms = entity.Room.CupboardsInRooms?
                        .Select(x => new App.BLL.DTO.CupboardInRoom()
                        {
                            Id = x.Id,
                            CupboardId = x.CupboardId,
                            RoomId = x.RoomId,
                        }).ToList()
                }
        };

        return res;
    }
}