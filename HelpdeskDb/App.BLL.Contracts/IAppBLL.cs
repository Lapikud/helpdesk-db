using App.BLL.Contracts.Identity;
using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface IAppBLL : IBaseBLL
{
    IAssetService AssetService { get; }
    
    ICategoryService CategoryService { get; }
    
    ICategoryAssetsService CategoryAssetsService { get; }
    
    ICupboardService CupboardService { get; }
    
    ICupboardInRoomService CupboardInRoomService { get; }
    
    ILocationService LocationService { get; }
    
    ILocationAssetsService LocationAssetsService { get; }
    
    ILocationInCupboardService LocationInCupboardService { get; }
    
    IOwnerService OwnerService { get; }
    
    IOwnerAssetsService OwnerAssetsService { get; }
    
    IRemovedAssetsService RemovedAssetsService { get; }
    
    IRoomService RoomService { get; }
    
    IAppUserRoleService AppUserRoleService { get; }
    
    IAssetReservationService AssetReservationService { get; }
}
