using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class AssetReservationUOWMapper: IMapper<App.DAL.DTO.AssetReservation, App.Domain.AssetReservation>
{
    public App.DAL.DTO.AssetReservation? Map(Domain.AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.AssetReservation()
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new App.DAL.DTO.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    SerialNumber = entity.Asset.SerialNumber,
                    Barcode = entity.Asset.Barcode,
                    // LastTakenBy = entity.Asset.LastTakenBy,
                    AssetReservations = entity.Asset.AssetReservations?
                        .Select(x => new DAL.DTO.AssetReservation()
                        {
                            Id = x.Id,
                            AssetId = x.AssetId,
                            UserId = x.UserId,
                            ReservationFrom =  x.ReservationFrom,
                            ReservationTo = x.ReservationTo,
                            IsReturned = x.IsReturned,
                        }).ToList()
                },
            UserId = entity.UserId,
            User = entity.User == null
                ? null
                : new App.DAL.DTO.Identity.AppUser()
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

    public Domain.AssetReservation? Map(AssetReservation? entity)
    {
        if (entity == null) return null;
        var res = new Domain.AssetReservation()
        {
            Id = entity.Id,
            AssetId = entity.AssetId,
            Asset = entity.Asset == null
                ? null
                : new Domain.Asset()
                {
                    Id = entity.Asset.Id,
                    AssetName = entity.Asset.AssetName,
                    Comment = entity.Asset.Comment,
                    SerialNumber = entity.Asset.SerialNumber,
                    Barcode = entity.Asset.Barcode,
                    // LastTakenBy = entity.Asset.LastTakenBy,
                    AssetReservations = entity.Asset.AssetReservations?
                        .Select(x => new Domain.AssetReservation()
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
                : new App.Domain.Identity.AppUser()
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