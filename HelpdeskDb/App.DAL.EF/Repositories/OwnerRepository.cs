using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class OwnerRepository : BaseRepository<App.DAL.DTO.Owner, App.Domain.Owner>, IOwnerRepository
{
    public OwnerRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new OwnerUOWMapper())
    {
    }

    public override async Task<IEnumerable<Owner>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(o => o.OwnerAssets)
            .OrderBy(o => o.OwnerName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }
}