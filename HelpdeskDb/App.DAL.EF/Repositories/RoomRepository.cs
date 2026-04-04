using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;

using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class RoomRepository : BaseRepository<App.DAL.DTO.Room, App.Domain.Room>, IRoomRepository
{
    public RoomRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new RoomUOWMapper())
    {
    }
    
    public override async Task<IEnumerable<Room>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(r => r.CupboardsInRooms)
            .OrderBy(o => o.RoomName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }
}