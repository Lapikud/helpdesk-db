using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class CupboardBLLMapper : IMapper<App.BLL.DTO.Cupboard, App.DAL.DTO.Cupboard>
{
    public App.DAL.DTO.Cupboard? Map(App.BLL.DTO.Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new App.DAL.DTO.CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new App.DAL.DTO.LocationInCupboard()
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

    public App.BLL.DTO.Cupboard? Map(App.DAL.DTO.Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
            CupboardsInRooms = entity.CupboardsInRooms?.Select(cir => new App.BLL.DTO.CupboardInRoom()
            {
                Id = cir.Id,
                Comment = cir.Comment,
                
                CupboardId = cir.CupboardId,
                Cupboard = null, // la.Asset,
                
                RoomId = cir.RoomId,
                Room = null,
                
            }).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new App.BLL.DTO.LocationInCupboard()
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