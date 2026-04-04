using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface ILocationInCupboardService : IBaseService<App.BLL.DTO.LocationInCupboard>
{
    Task<App.BLL.DTO.LocationInCupboard?> GetLocationInCupboardByLocationId(Guid locationId);
}