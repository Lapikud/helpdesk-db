using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.DAL.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class CategoryRepository : BaseRepository<App.DAL.DTO.Category, App.Domain.Category>, ICategoryRepository
{
    public CategoryRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new CategoryUOWMapper())
    {
    }
    
    public override async Task<IEnumerable<Category>> AllAsync(Guid userId = default)
    {
        var query = GetQuery();
        query = query
            .Include(x => x.CategoryAssetsCollection)
            .OrderBy(c => c.CategoryName);
        return (await query.ToListAsync()).Select(e => Mapper.Map(e)!);
    }
}