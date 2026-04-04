using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;

using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class OwnerAssetsRepository : BaseRepository<App.DAL.DTO.OwnerAssets, App.Domain.OwnerAssets>, IOwnerAssetsRepository
{
    public OwnerAssetsRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new OwnerAssetsUOWMapper())
    {
    }
    
    public override async Task<IEnumerable<OwnerAssets>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(oa => oa.Asset)
                    .ThenInclude(x => x!.OwnerAssets)
                .Include(oa => oa.Owner)
                    .ThenInclude(x => x!.OwnerAssets)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<OwnerAssets?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(oa => oa.Asset)
            .Include(oa => oa.Owner)
            .Where(oa => oa.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }
    
    public async Task<App.DAL.DTO.OwnerAssets?> GetOwnerAssetsByAssetId(Guid assetId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(oa => oa.AssetId.Equals(assetId)));
    }

    public async Task<OwnerAssets?> CreateNewOwnerAsset(Guid assetId, Guid ownerId)
    {
        var ownerAssets = await GetOwnerAssetsByAssetId(assetId);
        
        if (ownerAssets != null) return null;
        
        var ownerAsset = new App.DAL.DTO.OwnerAssets
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            OwnerId = ownerId,
        };
        
        var addedEntity = await this.AddAsync(ownerAsset);
        return addedEntity;
    }

    public async Task UpdateOwnerOfAsset(Guid? ownerAssetsId, Guid ownerId, Guid assetId = default)
    {
        if (ownerAssetsId == null)
        {
            var newOwnerAsset = new App.DAL.DTO.OwnerAssets()
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                OwnerId = ownerId,
            };
            await this.AddAsync(newOwnerAsset);
            return;
        }
        var query = base.GetQuery();
        var ownerAsset = await query.FirstOrDefaultAsync(oa => oa.Id.Equals(ownerAssetsId));
        if (ownerAsset == null) return;

        ownerAsset.OwnerId = ownerId;

        RepositoryDbContext.Entry(ownerAsset).Property(oa => oa.OwnerId).IsModified = true;
    }
}