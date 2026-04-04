using App.BLL.DTO.ViewModels;
using Base.Contracts;

namespace App.BLL.Mappers;

public class AssetViewModelMapper: IMapper<App.BLL.DTO.ViewModels.AssetViewModel, App.DAL.DTO.ViewModels.AssetViewModel>
{
    public DAL.DTO.ViewModels.AssetViewModel? Map(App.BLL.DTO.ViewModels.AssetViewModel? entity)
    {
        if (entity == null) return null;

        var res = new App.DAL.DTO.ViewModels.AssetViewModel()
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
    
    public App.BLL.DTO.ViewModels.AssetViewModel? Map(DAL.DTO.ViewModels.AssetViewModel? entity)
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