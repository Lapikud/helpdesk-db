using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class CategoryAssetsRepository : BaseRepository<App.DAL.DTO.CategoryAssets, App.Domain.CategoryAssets>,
    ICategoryAssetsRepository
{
    public CategoryAssetsRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new CategoryAssetsUOWMapper())
    {
    }

    public override async Task<IEnumerable<CategoryAssets>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(ca => ca.Asset)
                    .ThenInclude(a => a!.CategoryAssetsCollection)
                .Include(ca => ca.Category)
                    .ThenInclude(c => c!.CategoryAssetsCollection)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<CategoryAssets?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(ca => ca.Asset)
            .Include(ca => ca.Category)
            .Where(ca => ca.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }

    public async Task<App.DAL.DTO.CategoryAssets?> GetCategoryAssetsByAssetId(Guid assetId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(ca => ca.AssetId.Equals(assetId)));
    }

    public async Task UpdateCategoryOfAsset(Guid? categoryAssetsId, Guid categoryId, Guid assetId = default)
    {
        if (categoryAssetsId == null)
        {
            var newCategoryAsset = new App.DAL.DTO.CategoryAssets
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                CategoryId = categoryId,
                Comment = "idk why comment here"
            };
            await this.AddAsync(newCategoryAsset);
            return;
        }
        
        var query = base.GetQuery();
        var categoryAsset = await query
            .FirstOrDefaultAsync(ca => ca.Id.Equals(categoryAssetsId));

        if (categoryAsset == null) return;
        categoryAsset.CategoryId = categoryId;

        RepositoryDbContext.Entry(categoryAsset).Property(ca => ca.CategoryId).IsModified = true;
    }

    public async Task<App.DAL.DTO.CategoryAssets?> CreateNewCategoryAsset(Guid assetId, Guid categoryId)
    {
        var categoryAssets = await GetCategoryAssetsByAssetId(assetId);
        
        if (categoryAssets != null) return null;
        
        var categoryAsset = new App.DAL.DTO.CategoryAssets
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            CategoryId = categoryId,
            Comment = "idk why comment here"
        };

        var addedEntity = await this.AddAsync(categoryAsset);
        return addedEntity;
    }
}