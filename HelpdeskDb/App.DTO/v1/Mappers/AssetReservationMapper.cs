using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class AssetReservationMapper: IMapper<App.DTO.v1.AssetReservation, App.BLL.DTO.AssetReservation>
{
    public App.DTO.v1.AssetReservation? Map(BLL.DTO.AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.AssetReservation()
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            UserId = entity.UserId,
            ReservationFrom = entity.ReservationFrom,
            ReservationTo = entity.ReservationTo,
            IsReturned = entity.IsReturned,
        };

        return res;
    }

    public BLL.DTO.AssetReservation? Map(AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.AssetReservation()
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            UserId = entity.UserId,
            ReservationFrom = entity.ReservationFrom,
            ReservationTo = entity.ReservationTo,
            IsReturned = entity.IsReturned,
        };

        return res;
    }
    
    public BLL.DTO.AssetReservation Map(App.DTO.v1.CreateObjects.AssetReservationCreate entity)
    {
        var res = new App.BLL.DTO.AssetReservation()
        {
            Id = Guid.NewGuid(),
            AssetId = entity.AssetId,
            UserId = entity.UserId,
            ReservationFrom = entity.ReservationFrom,
            ReservationTo = entity.ReservationTo,
        };

        return res;
    }
}