using App.DAL.DTO;
using Base.Contracts;

namespace App.DAL.EF.Mappers;

public class LocationUOWMapper : IMapper<App.DAL.DTO.Location, App.Domain.Location>
{
    private readonly LocationInCupboardUOWMapper _licMapper = new();
    public Location? Map(Domain.Location? entity)
    {
        if (entity == null) return null;
        var res = new Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
            // LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => _licMapper.Map(lic)!).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new LocationAssets()
            {
                Id = la.Id,
                AssetId = la.AssetId,
                Asset = null, // la.Asset,
                
                LocationId = la.LocationId,
                Location = null,
                
                CreatedBy = la.CreatedBy
                
            }).ToList(),
        };
        return res;
    }

    public Domain.Location? Map(Location? entity)
    {
        if (entity == null) return null;
        var res = new Domain.Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
            // LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => _licMapper.Map(lic)!).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new Domain.LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new Domain.LocationAssets()
            {
                Id = la.Id,
                AssetId = la.AssetId,
                Asset = null, // la.Asset,
                
                LocationId = la.LocationId,
                Location = null,
                
                CreatedBy = la.CreatedBy
                
            }).ToList(),
        };
        return res;
    }
}