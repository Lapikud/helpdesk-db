using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class CupboardInRoomMapper : IMapper<App.DTO.v1.CupboardInRoom, App.BLL.DTO.CupboardInRoom>
{
    public App.DTO.v1.CupboardInRoom? Map(BLL.DTO.CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            RoomId = entity.RoomId,
        };

        return res;
    }

    public BLL.DTO.CupboardInRoom? Map(App.DTO.v1.CupboardInRoom? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.CupboardInRoom()
        {
            Id = entity.Id,
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            RoomId = entity.RoomId,
        };

        return res;
    }
    
    public BLL.DTO.CupboardInRoom Map(App.DTO.v1.CreateObjects.CupboardInRoomCreate entity)
    {

        var res = new App.BLL.DTO.CupboardInRoom()
        {
            Id = Guid.NewGuid(),
            Comment = entity.Comment,
            CupboardId = entity.CupboardId,
            RoomId = entity.RoomId,
        };

        return res;
    }
}