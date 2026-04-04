using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.BLL.Services;

public class AssetReservationService : BaseService<App.BLL.DTO.AssetReservation, App.DAL.DTO.AssetReservation, App.DAL.Contracts.IAssetReservationRepository>,
    IAssetReservationService
{
    public AssetReservationService(
        IAppUOW serviceUOW,
        IMapper<AssetReservation, DAL.DTO.AssetReservation> mapper) : base(serviceUOW, serviceUOW.AssetReservationRepository, mapper)
    {
    }

    public async Task UserReserveAsset(Guid userId, Guid assetId, DateTime reservationFrom, DateTime reservationTo)
    {
        await ServiceRepository.UserReserveAsset(userId, assetId, reservationFrom, reservationTo);
    }

    public async Task<bool> HasActiveOrFutureReservation(Guid assetId, Guid userId)
    {
        return await ServiceRepository.HasActiveOrFutureReservation(assetId, userId);
    }

    public async Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom, DateTime reservationTo)
    {
        return await ServiceRepository.IsAssetReservationAvailable(assetId, reservationFrom, reservationTo);
    }

    public async Task<bool> IsAssetReservationAvailable(Guid assetId, DateTime reservationFrom, DateTime reservationTo, Guid excludeReservationId)
    {
        return await ServiceRepository.IsAssetReservationAvailable(assetId, reservationFrom, reservationTo, excludeReservationId);
    }

    public async Task RemoveAssetReservation(Guid userId, Guid assetId)
    {
        await ServiceRepository.RemoveAssetReservation(userId, assetId);
    }

    public async Task<AssetReservation?> GetAssetReservationsByUserIdAndAssetId(Guid userId, Guid assetId)
    {
        var reservation = await ServiceRepository.GetAssetReservationsByUserIdAndAssetId(userId, assetId);
        return Mapper.Map(reservation);
    }

    public async Task AssetReturned(Guid userId, Guid assetId)
    {
        await ServiceRepository.AssetReturned(userId, assetId);
    }
}