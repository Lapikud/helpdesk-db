using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class LocationInCupboardService : BaseService<App.BLL.DTO.LocationInCupboard, App.DAL.DTO.LocationInCupboard,
        App.DAL.Contracts.ILocationInCupboardRepository>,
    ILocationInCupboardService
{
    public LocationInCupboardService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.LocationInCupboard, App.DAL.DTO.LocationInCupboard> mapper) : base(serviceUOW,
        serviceUOW.LocationInCupboardRepository, mapper)
    {
    }

    public async Task<LocationInCupboard?> GetLocationInCupboardByLocationId(Guid locationId)
    {
        var locationInCupboard = await ServiceRepository.GetLocationInCupboardByLocationId(locationId);
        return Mapper.Map(locationInCupboard);
    }
}