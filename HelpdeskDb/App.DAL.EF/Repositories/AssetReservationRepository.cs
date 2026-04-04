using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class AssetReservationRepository : BaseRepository<App.DAL.DTO.AssetReservation, App.Domain.AssetReservation>,
    IAssetReservationRepository
{
    public AssetReservationRepository(DbContext repositoryDbContext) : base(repositoryDbContext,
        new AssetReservationUOWMapper())
    {
    }

    public override async Task<IEnumerable<AssetReservation>> AllAsync(Guid userId = default)
    {
        var query = GetQuery(userId);
        query = query
            .Include(ra => ra.Asset)
            .ThenInclude(a => a!.AssetReservations)
            .Include(ar => ar.User)
            .ThenInclude(a => a!.AssetReservations);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }

    public override async Task<AssetReservation?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(ar => ar.Asset)
            .Include(ar => ar.User)
            .Where(ar => ar.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }

    public async Task UserReserveAsset(Guid userId, Guid assetId, DateTime reservationFrom, DateTime reservationTo)
    {
        var isAvailable = await IsAssetReservationAvailable(assetId, reservationFrom, reservationTo);

        if (!isAvailable) return;

        var newReservation = new App.DAL.DTO.AssetReservation()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssetId = assetId,
            ReservationFrom = reservationFrom,
            ReservationTo = reservationTo
        };

        await this.AddAsync(newReservation);
    }
    
    public async Task RemoveAssetReservation(Guid userId, Guid assetId)
    {
        var now = DateTime.UtcNow;
        var reservations = await GetQuery()
            .Where(ar => ar.UserId.Equals(userId) && ar.AssetId.Equals(assetId))
            .ToListAsync();

        // Prefer currently-active reservation; fall back to soonest upcoming
        var toRemove = reservations
                           .Where(ar => ar.ReservationFrom <= now && ar.ReservationTo >= now)
                           .OrderBy(ar => ar.ReservationFrom)
                           .FirstOrDefault()
                       ?? reservations
                           .Where(ar => ar.ReservationFrom > now)
                           .OrderBy(ar => ar.ReservationFrom)
                           .FirstOrDefault();

        if (toRemove == null) return;

        RepositoryDbContext.Set<App.Domain.AssetReservation>().Remove(toRemove);
    }

    public async Task<AssetReservation?> GetAssetReservationsByUserIdAndAssetId(Guid userId, Guid assetId)
    {
        return Mapper.Map(
            await RepositoryDbSet.FirstOrDefaultAsync(ar => ar.UserId.Equals(userId) && ar.AssetId.Equals(assetId)));
    }
    
    public async Task<bool> HasActiveOrFutureReservation(Guid assetId, Guid userId)
    {
        var now = DateTime.UtcNow;

        return await RepositoryDbSet.AnyAsync(r =>
            r.AssetId == assetId &&
            r.UserId == userId &&
            r.ReservationTo > now &&
            !r.IsReturned
        );
    }

    public async Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom,
        DateTime reservationTo)
    {
        if (reservationTo <= reservationFrom) return false;

        var buffer = TimeSpan.FromMinutes(10);

        var from = reservationFrom.Subtract(buffer);
        var to = reservationTo.Add(buffer);
        var now = DateTime.UtcNow;

        var hasConflict = await RepositoryDbSet.AnyAsync(r =>
            r.AssetId == assetId &&
            !r.IsReturned &&
            (r.ReservationFrom < to && r.ReservationTo > from || r.ReservationTo <= now)
        );

        Console.WriteLine("Has conflict: " + hasConflict);

        return !hasConflict;
    }

    public async Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom,
        DateTime reservationTo, Guid excludeReservationId)
    {
        if (reservationTo <= reservationFrom) return false;

        var buffer = TimeSpan.FromMinutes(10);

        var from = reservationFrom.Subtract(buffer);
        var to = reservationTo.Add(buffer);
        var now = DateTime.UtcNow;

        var hasConflict = await RepositoryDbSet.AnyAsync(r =>
            r.AssetId == assetId &&
            r.Id != excludeReservationId &&
            !r.IsReturned &&
            (r.ReservationFrom < to && r.ReservationTo > from || r.ReservationTo <= now)
        );

        Console.WriteLine("Has conflict: " + hasConflict);

        return !hasConflict;
    }

    public async Task AssetReturned(Guid userId, Guid assetId)
    {
        var now = DateTime.UtcNow;
        var reservation = await RepositoryDbSet
            .Where(ar => ar.UserId == userId && ar.AssetId == assetId && !ar.IsReturned)
            .OrderByDescending(ar => ar.ReservationFrom)
            .FirstOrDefaultAsync();

        if (reservation == null) return;

        reservation.IsReturned = true;
        if (reservation.ReservationTo > now)
        {
            reservation.ReservationTo = now;
        }
        
        RepositoryDbContext.Set<App.Domain.AssetReservation>().Update(reservation);
    }
}