using App.DAL.Contracts;
using App.DAL.Contracts.Identity;
using App.DAL.DTO.Identity;
using App.DAL.EF.Mappers.Identity;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories.Identity;

public class AppUserRoleRepository : BaseRepository<App.DAL.DTO.Identity.AppUserRole, App.Domain.Identity.AppUserRole>,
    IAppUserRoleRepository
{
    public AppUserRoleRepository(AppDbContext repositoryDbContext) : base(repositoryDbContext,
        new AppUserRoleUOWMapper())
    {
    }

    public override async Task<IEnumerable<App.DAL.DTO.Identity.AppUserRole>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<App.DAL.DTO.Identity.AppUserRole?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .Where(ur => ur.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }
}