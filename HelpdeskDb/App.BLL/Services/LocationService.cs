using App.BLL.Contracts;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class LocationService : BaseService<App.BLL.DTO.Location, App.DAL.DTO.Location, App.DAL.Contracts.ILocationRepository>,
    ILocationService
{
    public LocationService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.Location, App.DAL.DTO.Location> mapper) : base(serviceUOW, serviceUOW.LocationRepository, mapper)
    {
    }
}