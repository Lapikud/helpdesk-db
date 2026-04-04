using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface ILocationInCupboardRepository : IBaseRepository<App.DAL.DTO.LocationInCupboard>
{
    Task<App.DAL.DTO.LocationInCupboard?> GetLocationInCupboardByLocationId(Guid locationId);
}