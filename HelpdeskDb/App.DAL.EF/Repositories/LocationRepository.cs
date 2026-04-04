using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class LocationRepository : BaseRepository<App.DAL.DTO.Location, App.Domain.Location>, ILocationRepository
{
    public LocationRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new LocationUOWMapper())
    {
    }

    public override async Task<IEnumerable<Location>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(l => l.LocationsInCupboards)
            .Include(l => l.LocationsAssetsCollection)
            .OrderBy(l => l.LocationName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }
}