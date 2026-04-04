using Base.Contracts;

namespace App.DAL.DTO;

public class Asset: IDomainId
{
    public Guid Id { get; set; }

    public string AssetName { get; set; } = default!;

    public string Comment { get; set; } = default!;

    public string? SerialNumber { get; set; }

    public string? Barcode { get; set; }

    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
    public ICollection<OwnerAssets>? OwnerAssets { get; set; }
    public ICollection<CategoryAssets>? CategoryAssetsCollection { get; set; }
    public ICollection<RemovedAssets>? RemovedAssetsCollection { get; set; }
    public ICollection<AssetReservation>? AssetReservations { get; set; }
}
