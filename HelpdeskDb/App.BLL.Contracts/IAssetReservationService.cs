using App.DAL.Contracts;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface IAssetReservationService: IBaseService<App.BLL.DTO.AssetReservation>, IAssetReservationRepositoryCustom
{
    Task<App.BLL.DTO.AssetReservation?> GetAssetReservationsByUserIdAndAssetId(Guid userId, Guid assetId);
}