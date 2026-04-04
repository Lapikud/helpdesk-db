using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DAL.DTO;

public class LocationAssets : IDomainId
{
    public Guid Id { get; set; }
    
    public string CreatedBy = "System";

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }

    public Guid LocationId { get; set; }

    public Location? Location { get; set; }
}