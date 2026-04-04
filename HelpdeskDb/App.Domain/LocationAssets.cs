using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class LocationAssets : BaseEntity
{
    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }

    public Guid LocationId { get; set; }

    public Location? Location { get; set; }
}