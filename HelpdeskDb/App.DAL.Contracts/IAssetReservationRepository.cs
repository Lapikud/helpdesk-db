using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface IAssetReservationRepository : IBaseRepository<App.DAL.DTO.AssetReservation>,
    IAssetReservationRepositoryCustom
{
    Task<App.DAL.DTO.AssetReservation?> GetAssetReservationsByUserIdAndAssetId(Guid userId, Guid assetId);
}

public interface IAssetReservationRepositoryCustom
{
    Task UserReserveAsset(Guid userId, Guid assetId, DateTime reservationFrom, DateTime reservationTo);

    Task<bool> HasActiveOrFutureReservation(Guid assetId, Guid userId);

    Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom, DateTime reservationTo);

    Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom, DateTime reservationTo, Guid excludeReservationId);

    Task RemoveAssetReservation(Guid userId, Guid assetId);

    Task AssetReturned(Guid userId, Guid assetId);
}