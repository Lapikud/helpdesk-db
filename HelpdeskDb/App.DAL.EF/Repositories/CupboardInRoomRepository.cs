using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class CupboardInRoomRepository : BaseRepository<App.DAL.DTO.CupboardInRoom, App.Domain.CupboardInRoom>,
    ICupboardInRoomRepository
{
    public CupboardInRoomRepository(DbContext repositoryDbContext) : base(repositoryDbContext,
        new CupboardInRoomUOWMapper())
    {
    }

    public override async Task<IEnumerable<CupboardInRoom>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(cir => cir.Cupboard)
                    .ThenInclude(x => x!.CupboardsInRooms)
                .Include(cir => cir.Room)
                    .ThenInclude(x => x!.CupboardsInRooms)
                .OrderBy(cir => cir.Cupboard!.CodeName)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<CupboardInRoom?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(cir => cir.Cupboard)
            .Include(cir => cir.Room)
            .Where(cir => cir.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }

    public async Task<App.DAL.DTO.CupboardInRoom?> GetCupboardInRoomByCupboardId(Guid cupboardId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(cir => cir.CupboardId.Equals(cupboardId)));
    }
}