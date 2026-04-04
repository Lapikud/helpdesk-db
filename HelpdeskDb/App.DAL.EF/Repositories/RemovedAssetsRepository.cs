using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;

using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class RemovedAssetsRepository : BaseRepository<App.DAL.DTO.RemovedAssets, App.Domain.RemovedAssets>,
    IRemovedAssetsRepository
{
    public RemovedAssetsRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new RemovedAssetsUOWMapper())
    {
    }
    
    public override async Task<IEnumerable<RemovedAssets>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(ua => ua.Asset)
                    .ThenInclude(x => x!.RemovedAssetsCollection)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<RemovedAssets?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(ua => ua.Asset)
            .Where(ua => ua.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }
    
    public async Task<App.DAL.DTO.RemovedAssets?> GetRemovedAssetByAssetId(Guid assetId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(ra => ra.AssetId.Equals(assetId)));
    }
    
    public async Task<App.DAL.DTO.RemovedAssets?> CreateNewRemovedAsset(Guid assetId, string comment)
    {
        var removedAssets = await GetRemovedAssetByAssetId(assetId);
        
        if (removedAssets != null) return null;
        
        var removedAsset = new App.DAL.DTO.RemovedAssets()
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            Comment = comment
        };

        var addedEntity = await this.AddAsync(removedAsset);
        return addedEntity;
    }
}