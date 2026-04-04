using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class RoomMapper : IMapper<App.DTO.v1.Room, App.BLL.DTO.Room>
{
    public App.DTO.v1.Room? Map(BLL.DTO.Room? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.Room()
        {
            Id = entity.Id,
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            
        };
        return res;
    }

    public BLL.DTO.Room? Map(App.DTO.v1.Room? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Room()
        {
            Id = entity.Id,
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            
        };
        return res;
    }
    
    public BLL.DTO.Room Map(App.DTO.v1.CreateObjects.RoomCreate entity)
    {
        var res = new App.BLL.DTO.Room()
        {
            Id = Guid.NewGuid(),
            RoomName = entity.RoomName,
            Comment = entity.Comment,
            
        };
        return res;
    }
}