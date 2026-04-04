using App.DAL.Contracts;
using App.DAL.DTO.ViewModels;
using App.DAL.EF.Mappers;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Asset = App.DAL.DTO.Asset;

namespace App.DAL.EF.Repositories;

public class AssetRepository : BaseRepository<App.DAL.DTO.Asset, App.Domain.Asset>, IAssetRepository
{
    public AssetRepository(AppDbContext repositoryDbContext) : base(repositoryDbContext, new AssetUOWMapper())
    {
    }

    public async Task<App.DAL.DTO.Asset> CreateNewAsset(string assetName, string comment, string? serialNumber = null, string? barcode = null)
    {
        var asset = new App.DAL.DTO.Asset
        {
            Id = Guid.NewGuid(),
            AssetName = assetName,
            Comment = comment,
            SerialNumber = serialNumber,
            Barcode = barcode,
        };
        var addedAsset = await this.AddAsync(asset);
        return addedAsset;
    }

    public async Task<App.DAL.DTO.ViewModels.AssetViewModel?> GetAssetVmByAssetId(Guid assetId)
    {
        var query = base.GetQuery();
        var assetVm = await query
            .Include(a => a.OwnerAssets)! // Load OwnerAssets
            .ThenInclude(oa => oa.Owner) // Load Owner
            .Include(a => a.LocationsAssetsCollection)! // Load LocationAssets
            .ThenInclude(la => la.Location) // Load Location
            .ThenInclude(l => l!.LocationsInCupboards)! // Load LocationInCupboard
            .ThenInclude(lc => lc.Cupboard) // Load Cupboard
            .ThenInclude(c => c!.CupboardsInRooms)! // Load CupboardInRoom
            .ThenInclude(cr => cr.Room) // Load Room
            .Include(a => a.CategoryAssetsCollection)! // Load CategoryAssets
            .ThenInclude(ca => ca.Category) // Load Category
            .Include(a => a.AssetReservations) // Load AssetReservations
            .Where(a => a.Id == assetId)
            .Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Barcode = a.Barcode,
                CategoryName = a.CategoryAssetsCollection!
                    .Select(ca => ca.Category!.CategoryName)
                    .First(),
                OwnerName = a.OwnerAssets!.Select(oa => oa.Owner!.OwnerName).First(),
                RoomName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .SelectMany(lc => lc.Cupboard!.CupboardsInRooms!)
                    .Select(cr => cr.Room!.RoomName)
                    .First(),
                CupboardName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .Select(lc => lc.Cupboard!.CodeName)
                    .First(),
                ShelfNum = a.LocationsAssetsCollection!.Select(la => la.Location!.ShelfNum).FirstOrDefault(),
                Column = a.LocationsAssetsCollection!.Select(la => la.Location!.Column).FirstOrDefault(),
                // LastTakenBy = a.LastTakenBy,
                ClosestReservationBy = "",
                AddedAt = a.LocationsAssetsCollection!.Select(la => la.CreatedAt).FirstOrDefault()
            }).FirstOrDefaultAsync();

        return assetVm;
    }

    public async Task<List<App.DAL.DTO.ViewModels.AssetViewModel>> GetAvailableAssets()
    {
        var query = base.GetQuery();
        var now = DateTime.UtcNow;
        return await query
            .Include(a => a.OwnerAssets)! // Load OwnerAssets
            .ThenInclude(oa => oa.Owner) // Load Owner
            .Include(a => a.LocationsAssetsCollection)! // Load LocationAssets
            .ThenInclude(la => la.Location) // Load Location
            .ThenInclude(l => l!.LocationsInCupboards)! // Load LocationInCupboard
            .ThenInclude(lc => lc.Cupboard) // Load Cupboard
            .ThenInclude(c => c!.CupboardsInRooms)! // Load CupboardInRoom
            .ThenInclude(cr => cr.Room) // Load Room
            .Include(a => a.CategoryAssetsCollection)! // Load CategoryAssets
            .ThenInclude(ca => ca.Category) // Load Category
            .Include(a => a.AssetReservations)! // Load AssetReservations
            .ThenInclude(ar => ar.User)
            .Where(a => (a.RemovedAssetsCollection == null || !a.RemovedAssetsCollection.Any()))
            .Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Barcode = a.Barcode,
                CategoryName = a.CategoryAssetsCollection!
                    .Select(ca => ca.Category!.CategoryName)
                    .First(),
                OwnerName = a.OwnerAssets!.Select(oa => oa.Owner!.OwnerName).First(),
                RoomName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .SelectMany(lc => lc.Cupboard!.CupboardsInRooms!)
                    .Select(cr => cr.Room!.RoomName)
                    .First(),
                CupboardName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .Select(lc => lc.Cupboard!.CodeName)
                    .First(),
                ShelfNum = a.LocationsAssetsCollection!.Select(la => la.Location!.ShelfNum).FirstOrDefault(),
                Column = a.LocationsAssetsCollection!.Select(la => la.Location!.Column).FirstOrDefault(),
                ClosestReservationBy = a.AssetReservations!.Where(ar => !ar.IsReturned && ar.ReservationTo >= now).Select(ar => ar.User!.Username).FirstOrDefault() ?? "-",
                AddedAt = a.LocationsAssetsCollection!.Select(la => la.CreatedAt).FirstOrDefault(),
                Reserved = a.AssetReservations!.Any(ar => !ar.IsReturned && ar.ReservationTo <= now)
            })
            .ToListAsync();
    }

    public async Task<List<App.DAL.DTO.ViewModels.AssetViewModel>> GetAssetsReservedByUser(Guid userId)
    {
        var query = base.GetQuery();
        return await query
            .Include(a => a.OwnerAssets)! // Load OwnerAssets
            .ThenInclude(oa => oa.Owner) // Load Owner
            .Include(a => a.LocationsAssetsCollection)! // Load LocationAssets
            .ThenInclude(la => la.Location) // Load Location
            .ThenInclude(l => l!.LocationsInCupboards)! // Load LocationInCupboard
            .ThenInclude(lc => lc.Cupboard) // Load Cupboard
            .ThenInclude(c => c!.CupboardsInRooms)! // Load CupboardInRoom
            .ThenInclude(cr => cr.Room) // Load Room
            .Include(a => a.CategoryAssetsCollection)! // Load CategoryAssets
            .ThenInclude(ca => ca.Category) // Load Category
            .Include(a => a.AssetReservations)! // Load assetReservations
            .ThenInclude(ar => ar.User)
            .Where(a => a.AssetReservations != null &&
                        a.AssetReservations.Any(ar => ar.UserId == userId && !ar.IsReturned) &&
                        (a.RemovedAssetsCollection == null || !a.RemovedAssetsCollection.Any()))
            .Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Barcode = a.Barcode,
                CategoryName = a.CategoryAssetsCollection!
                    .Select(ca => ca.Category!.CategoryName)
                    .First(),
                OwnerName = a.OwnerAssets!.Select(oa => oa.Owner!.OwnerName).First(),
                RoomName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .SelectMany(lc => lc.Cupboard!.CupboardsInRooms!)
                    .Select(cr => cr.Room!.RoomName)
                    .First(),
                CupboardName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .Select(lc => lc.Cupboard!.CodeName)
                    .First(),
                ShelfNum = a.LocationsAssetsCollection!.Select(la => la.Location!.ShelfNum).FirstOrDefault(),
                Column = a.LocationsAssetsCollection!.Select(la => la.Location!.Column).FirstOrDefault(),
                ClosestReservationBy = a.AssetReservations!.Where(ar => !ar.IsReturned).Select(ar => ar.User!.Username).FirstOrDefault() ?? "-",
                AddedAt = a.LocationsAssetsCollection!.Select(la => la.CreatedAt).FirstOrDefault(),
                Reserved = true,
                ReservationId = a.AssetReservations!
                    .Where(ar => ar.UserId == userId && !ar.IsReturned)
                    .OrderByDescending(ar => ar.ReservationFrom)
                    .Select(ar => ar.Id)
                    .First(),
                ReservationTo = a.AssetReservations!
                    .Where(ar => ar.UserId == userId && !ar.IsReturned)
                    .OrderByDescending(ar => ar.ReservationFrom)
                    .Select(ar => (DateTime?)ar.ReservationTo)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public override async Task<IEnumerable<Asset>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(x => x.CategoryAssetsCollection)
            .Include(x => x.LocationsAssetsCollection)
            .Include(x => x.OwnerAssets)
            .Include(x => x.RemovedAssetsCollection)
            .Include(x => x.AssetReservations)
            .OrderBy(a => a.AssetName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }

    public async Task<List<App.DAL.DTO.Asset>> GetAllNotRemovedAssets()
    {
        var notRemoved = (await this.AllAsync())
            .Where(a => (a.RemovedAssetsCollection == null || !a.RemovedAssetsCollection.Any())).ToList();
        return notRemoved;
    }
}