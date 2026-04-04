using App.DAL.Contracts;
using App.DAL.DTO;
using App.DAL.EF.Mappers;

using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class LocationAssetsRepository : BaseRepository<App.DAL.DTO.LocationAssets, App.Domain.LocationAssets>,
    ILocationAssetsRepository
{
    public LocationAssetsRepository(DbContext repositoryDbContext) : base(repositoryDbContext, new LocationAssetsUOWMapper())
    {
    }
    
    public override async Task<IEnumerable<LocationAssets>> AllAsync(Guid userId = default)
    {
        return (await RepositoryDbSet
                .Include(la => la.Asset)
                    .ThenInclude(x => x!.LocationsAssetsCollection)
                .Include(la => la.Location)
                    .ThenInclude(x => x!.LocationsAssetsCollection)
                .ToListAsync())
            .Select(e => Mapper.Map(e)!);
    }

    public override async Task<LocationAssets?> FindAsync(Guid id, Guid userId = default)
    {
        return Mapper.Map(await RepositoryDbSet
            .Include(la => la.Asset)
            .Include(la => la.Location)
            .Where(la => la.Id.Equals(id))
            .FirstOrDefaultAsync()
        );
    }
    
    
    public async Task<App.DAL.DTO.LocationAssets?> GetLocationAssetsByAssetId(Guid assetId)
    {
        return Mapper.Map(await RepositoryDbSet.FirstOrDefaultAsync(la => la.AssetId.Equals(assetId)));
    }

    public async Task<App.DAL.DTO.LocationAssets?> CreateNewLocationAsset(Guid assetId, Guid locationId)
    {
        var locationAssets = await GetLocationAssetsByAssetId(assetId);
        
        if (locationAssets != null) return null;
        
        var locationAsset = new App.DAL.DTO.LocationAssets
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            LocationId = locationId,
        };
        
        var addedEntity = await this.AddAsync(locationAsset);
        return addedEntity;
    }

    public async Task UpdateLocationOfAsset(Guid? locationAssetId, Guid locationId, Guid assetId = default)
    {
        if (locationAssetId == null)
        {
            var newLocationAsset = new App.DAL.DTO.LocationAssets()
            {
                Id = Guid.NewGuid(),
                AssetId = assetId,
                LocationId = locationId,
            };
            await this.AddAsync(newLocationAsset);
            return;
        }
        var query = base.GetQuery();
        var locationAsset = await query
            .FirstOrDefaultAsync(la => la.Id.Equals(locationAssetId));

        if (locationAsset == null) return;

        locationAsset.LocationId = locationId;

        RepositoryDbContext.Entry(locationAsset).Property(la => la.LocationId).IsModified = true;
    }
}