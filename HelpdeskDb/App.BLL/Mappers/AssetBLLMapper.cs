using App.DAL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class AssetBLLMapper: IMapper<App.BLL.DTO.Asset, App.DAL.DTO.Asset>
{
    public App.DAL.DTO.Asset? Map(App.BLL.DTO.Asset? entity)
    {
        if (entity == null) return null;

        var res = new App.DAL.DTO.Asset()
        {
            Id = entity.Id,
            AssetName = entity.AssetName,
            Comment = entity.Comment,
            SerialNumber = entity.SerialNumber,
            Barcode = entity.Barcode,
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new App.DAL.DTO.LocationAssets()
            {
                Id = la.Id,
                AssetId = la.AssetId,
                Asset = null, // la.Asset,
                
                LocationId = la.LocationId,
                Location = null,
                
                CreatedBy = la.CreatedBy
                
            }).ToList(),
            OwnerAssets = entity.OwnerAssets?.Select(oa => new App.DAL.DTO.OwnerAssets()
            {
                Id = oa.Id,
                AssetId = oa.AssetId,
                Asset = null, // la.Asset,
                
                OwnerId = oa.OwnerId,
                Owner = null,
                CreatedBy = oa.CreatedBy
            }).ToList(),
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new App.DAL.DTO.CategoryAssets()
            {
                Id = ca.Id,
                Comment = ca.Comment,
                AssetId = ca.AssetId,
                Asset = null, // la.Asset,
                
                CategoryId = ca.CategoryId,
                Category = null,
                
                CreatedBy = ca.CreatedBy,
            }).ToList(),
            RemovedAssetsCollection = entity.RemovedAssetsCollection?.Select(ra => new App.DAL.DTO.RemovedAssets()
            {
                Id = ra.Id,
                AssetId = ra.AssetId,
                Asset = null, // la.Asset,
                Comment = ra.Comment,
            }).ToList(),
            AssetReservations = entity.AssetReservations?.Select(ar => new AssetReservation()
            {
                Id = ar.Id,
                AssetId = ar.AssetId,
                Asset = null, // la.Asset,
                
                UserId = ar.UserId,
                User = null,
                ReservationFrom = ar.ReservationFrom,
                ReservationTo = ar.ReservationTo,
                IsReturned = ar.IsReturned,
            }).ToList(),
        };
        return res;
    }

    public App.BLL.DTO.Asset? Map(App.DAL.DTO.Asset? entity)
    {
        if (entity == null) return null;

        var res = new App.BLL.DTO.Asset()
        {
            Id = entity.Id,
            AssetName = entity.AssetName,
            Comment = entity.Comment,
            SerialNumber = entity.SerialNumber,
            Barcode = entity.Barcode,
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new App.BLL.DTO.LocationAssets()
            {
                Id = la.Id,
                AssetId = la.AssetId,
                Asset = null, // la.Asset,
                
                LocationId = la.LocationId,
                Location = null,
                
                CreatedBy = la.CreatedBy
                
            }).ToList(),
            OwnerAssets = entity.OwnerAssets?.Select(oa => new App.BLL.DTO.OwnerAssets()
            {
                Id = oa.Id,
                AssetId = oa.AssetId,
                Asset = null, // la.Asset,
                
                OwnerId = oa.OwnerId,
                Owner = null,
                CreatedBy = oa.CreatedBy
            }).ToList(),
            CategoryAssetsCollection = entity.CategoryAssetsCollection?.Select(ca => new App.BLL.DTO.CategoryAssets()
            {
                Id = ca.Id,
                Comment = ca.Comment,
                AssetId = ca.AssetId,
                Asset = null, // la.Asset,
                
                CategoryId = ca.CategoryId,
                Category = null,
                
                CreatedBy = ca.CreatedBy,
            }).ToList(),
            RemovedAssetsCollection = entity.RemovedAssetsCollection?.Select(ra => new App.BLL.DTO.RemovedAssets()
            {
                Id = ra.Id,
                AssetId = ra.AssetId,
                Asset = null, // la.Asset,
                Comment = ra.Comment,
            }).ToList(),
            AssetReservations = entity.AssetReservations?.Select(ar => new App.BLL.DTO.AssetReservation()
            {
                Id = ar.Id,
                AssetId = ar.AssetId,
                Asset = null, // la.Asset,
                
                UserId = ar.UserId,
                User = null,
                ReservationFrom = ar.ReservationFrom,
                ReservationTo = ar.ReservationTo,
                IsReturned = ar.IsReturned,
            }).ToList(),
        };
        return res;
    }
}