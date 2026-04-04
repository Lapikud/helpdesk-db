using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;

using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class LocationInCupboardRepository :
    BaseRepository<App.DAL.DTO.LocationInCupboard, App.Domain.LocationInCupboard>, ILocationInCupboardRepository
{
    public LocationInCupboardRepository(DbContext repositoryDbContext) : base(repositoryDbContext,
        new LocationInCupboardUOWMapper())
    {
    }

    public override async Task<IEnumerable<LocationInCupboard>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(lic => lic.Cupboard)
                .ThenInclude(c => c!.LocationsInCupboards)
            .Include(lic => lic.Location)
                .ThenInclude(l => l!.LocationsInCupboards)
            .OrderBy(l => l.Location!.LocationName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }

    public override async Task<LocationInCupboard?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(lic => lic.Cupboard)
            .Include(lic => lic.Location)
            .Where(lic => lic.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }
    
    public async Task<App.DAL.DTO.LocationInCupboard?> GetLocationInCupboardByLocationId(Guid locationId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(lic => lic.LocationId.Equals(locationId)));
    }
}