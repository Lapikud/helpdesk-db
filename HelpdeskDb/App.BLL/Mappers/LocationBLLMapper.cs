using App.BLL.DTO;
using Base.Contracts;

namespace App.BLL.Mappers;

public class LocationBLLMapper : IMapper<App.BLL.DTO.Location, App.DAL.DTO.Location>
{
    private readonly LocationInCupboardBLLMapper _licMapper = new();
    public App.DAL.DTO.Location? Map(App.BLL.DTO.Location? entity)
    {
        if (entity == null) return null;
        var res = new App.DAL.DTO.Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
            // LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => _licMapper.Map(lic)!).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new App.DAL.DTO.LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new App.DAL.DTO.LocationAssets()
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

    public App.BLL.DTO.Location? Map(App.DAL.DTO.Location? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Location()
        {
            Id = entity.Id,
            LocationName = entity.LocationName,
            ShelfNum = entity.ShelfNum,
            Column = entity.Column,
            // LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => _licMapper.Map(lic)!).ToList(),
            LocationsInCupboards = entity.LocationsInCupboards?.Select(lic => new App.BLL.DTO.LocationInCupboard()
            {
                Id = lic.Id,
                LocationId = lic.LocationId,
                Location = null,
                
                CupboardId = lic.CupboardId,
                Cupboard = null,
            }).ToList(),
            LocationsAssetsCollection = entity.LocationsAssetsCollection?.Select(la => new App.BLL.DTO.LocationAssets()
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