using Base.Contracts;

namespace App.BLL.Mappers;

public class AssetReservationBLLMapper : IMapper<App.BLL.DTO.AssetReservation, App.DAL.DTO.AssetReservation>
{
    public App.DAL.DTO.AssetReservation? Map(App.BLL.DTO.AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.AssetReservation()
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

    public App.BLL.DTO.AssetReservation? Map(App.DAL.DTO.AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.AssetReservation()
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.BLL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    SerialNumber = entity.Asset.SerialNumber,
                    Barcode = entity.Asset.Barcode,
                    // // LastTakenBy = entity.Asset.LastTakenBy,
                    AssetReservations = entity.Asset.AssetReservations?
                        .Select(x => new App.BLL.DTO.AssetReservation()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            UserId = x.UserId,
                            ReservationFrom = x.ReservationFrom,
                            ReservationTo = x.ReservationTo,
                            IsReturned = x.IsReturned,
                        }).ToList()
                },
            UserId = entity.UserId,
            User = entity.User == null
                ? null
                : new App.BLL.DTO.Identity.AppUser()
                {
                    Id = entity.User.Id,
                    Username = entity.User.Username!,
                },
            ReservationFrom = entity.ReservationFrom,
            ReservationTo = entity.ReservationTo,
            IsReturned = entity.IsReturned,
        };

        return res;
    }
}