using App.DAL.Contracts;
using App.DAL.Contracts.Identity;
using App.DAL.EF.Repositories;
using App.DAL.EF.Repositories.Identity;
using Base.DAL.EF;

namespace App.DAL.EF;

public class AppUOW : BaseUOW<AppDbContext>, IAppUOW
{
    // private readonly IMapper _mapper;

    public AppUOW(AppDbContext uowDbContext) : base(uowDbContext)
    {
        // _mapper = mapper;
    }

    // Assets
    private IAssetRepository? _assetRepository;
    public IAssetRepository AssetRepository => 
        _assetRepository ??= new AssetRepository(UOWDbContext);

    // Category
    private ICategoryRepository? _categoryRepository;
    public ICategoryRepository CategoryRepository =>
        _categoryRepository ??= new CategoryRepository(UOWDbContext);

    // CategoryAssets
    private ICategoryAssetsRepository? _categoryAssetsRepository;
    public ICategoryAssetsRepository CategoryAssetsRepository =>
        _categoryAssetsRepository ??= new CategoryAssetsRepository(UOWDbContext);
    
    // Cupboard
    private ICupboardRepository? _cupboardRepository;
    public ICupboardRepository CupboardRepository =>
        _cupboardRepository ??= new CupboardRepository(UOWDbContext);
    
    // CupboardInRoom
    private ICupboardInRoomRepository? _cupboardInRoomRepository;
    public ICupboardInRoomRepository CupboardInRoomRepository =>
        _cupboardInRoomRepository ??= new CupboardInRoomRepository(UOWDbContext);

    // Location
    private ILocationRepository? _locationRepository;
    public ILocationRepository LocationRepository =>
        _locationRepository ??= new LocationRepository(UOWDbContext);
    
    // LocationInCupboard
    private ILocationInCupboardRepository? _locationInCupboardRepository;
    public ILocationInCupboardRepository LocationInCupboardRepository =>
        _locationInCupboardRepository ??= new LocationInCupboardRepository(UOWDbContext);
    
    // LocationAssets
    private ILocationAssetsRepository? _locationAssetsRepository;
    public ILocationAssetsRepository LocationAssetsRepository =>
        _locationAssetsRepository ??= new LocationAssetsRepository(UOWDbContext);
    
    // Owner
    private IOwnerRepository? _ownerRepository;
    public IOwnerRepository OwnerRepository =>
        _ownerRepository ??= new OwnerRepository(UOWDbContext);
    
    // OwnerAssets
    private IOwnerAssetsRepository? _ownerAssetsRepository;
    public IOwnerAssetsRepository OwnerAssetsRepository =>
        _ownerAssetsRepository ??= new OwnerAssetsRepository(UOWDbContext);
    
    // RemovedAssets
    private IRemovedAssetsRepository? _removedAssetsRepository;
    public IRemovedAssetsRepository RemovedAssetsRepository =>
        _removedAssetsRepository ??= new RemovedAssetsRepository(UOWDbContext);
    
    // Room
    private IRoomRepository? _roomRepository;
    public IRoomRepository RoomRepository =>
        _roomRepository ??= new RoomRepository(UOWDbContext);
    
    
    // UserRoles
    private IAppUserRoleRepository? _appUserRoleRepository;
    public IAppUserRoleRepository AppUserRoleRepository =>
        _appUserRoleRepository ??= new AppUserRoleRepository(UOWDbContext);
    
    // AssetReservation
    private IAssetReservationRepository? _assetReservationRepository;
    public IAssetReservationRepository AssetReservationRepository =>
        _assetReservationRepository ??= new AssetReservationRepository(UOWDbContext);
}