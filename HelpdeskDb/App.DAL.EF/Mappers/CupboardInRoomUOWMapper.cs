using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class CupboardInRoomUOWMapper : IMapper<App.DAL.DTO.CupboardInRoom, App.Domain.CupboardInRoom>
{
    public CupboardInRoom? Map(Domain.CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    CupboardsInRooms = entity.Cupboard.CupboardsInRooms?
                        .Select(x => new App.DAL.DTO.CupboardInRoom()
                        {
                            Id = x.Id,
                            CupboardId = x.CupboardId,
                            RoomId = x.RoomId,
                        }).ToList()
                },
            RoomId = entity.RoomId,
            Room = entity.Room == null
                ? null
                : new Room()
                {
                    Id = entity.Room!.Id,
                    RoomName = entity.Room!.RoomName,
                    Comment = entity.Room!.Comment,
                    CupboardsInRooms = entity.Room.CupboardsInRooms?
                        .Select(x => new App.DAL.DTO.CupboardInRoom()
                        {
                            Id = x.Id,
                            CupboardId = x.CupboardId,
                            RoomId = x.RoomId,
                        }).ToList()
                }
        };

        return res;
    }

    public Domain.CupboardInRoom? Map(CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.Domain.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            Cupboard = entity.Cupboard == null
                ? null
                : new App.Domain.Cupboard()
                {
                    Id = entity.Cupboard.Id,
                    CodeName = entity.Cupboard.CodeName,
                    CupboardsInRooms = entity.Cupboard.CupboardsInRooms?
                        .Select(x => new Domain.CupboardInRoom()
                        {
                            Id = x.Id,
                            CupboardId = x.CupboardId,
                            RoomId = x.RoomId,
                        }).ToList()
                },
            RoomId = entity.RoomId,
            Room = entity.Room == null
                ? null
                : new App.Domain.Room()
                {
                    Id = entity.Room!.Id,
                    RoomName = entity.Room!.RoomName,
                    Comment = entity.Room!.Comment,
                    CupboardsInRooms = entity.Room.CupboardsInRooms?
                        .Select(x => new Domain.CupboardInRoom()
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