using App.DTO.v1.ViewModels;
using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class AssetViewModelMapper : IMapper<App.DTO.v1.ViewModels.AssetViewModel, App.BLL.DTO.ViewModels.AssetViewModel>
{
    public AssetViewModel? Map(BLL.DTO.ViewModels.AssetViewModel? entity)
    {
        if (entity == null) return null;

        var res = new App.DTO.v1.ViewModels.AssetViewModel()
        {
            Id = entity.Id, // asset id
            AssetName = entity.AssetName,
            SerialNumber = entity.SerialNumber,
            Barcode = entity.Barcode,
            CategoryName = entity.CategoryName,
            OwnerName = entity.OwnerName,
            RoomName = entity.RoomName,
            CupboardName = entity.CupboardName,
            Column = entity.Column,
            ShelfNum = entity.ShelfNum,
            ClosestReservationBy = entity.ClosestReservationBy,
            AddedAt = entity.AddedAt,
            Reserved = entity.Reserved,
            ReservationId = entity.ReservationId,
            ReservationTo = entity.ReservationTo
        };
        return res;
    }

    public BLL.DTO.ViewModels.AssetViewModel? Map(AssetViewModel? entity)
    {
        if (entity == null) return null;

        var res = new App.BLL.DTO.ViewModels.AssetViewModel()
        {
            Id = entity.Id, // asset id
            AssetName = entity.AssetName,
            SerialNumber = entity.SerialNumber,
            Barcode = entity.Barcode,
            CategoryName = entity.CategoryName,
            OwnerName = entity.OwnerName,
            RoomName = entity.RoomName,
            CupboardName = entity.CupboardName,
            Column = entity.Column,
            ShelfNum = entity.ShelfNum,
            ClosestReservationBy = entity.ClosestReservationBy,
            AddedAt = entity.AddedAt,
            Reserved = entity.Reserved,
            ReservationId = entity.ReservationId,
            ReservationTo = entity.ReservationTo
        };
        return res;
    }
}