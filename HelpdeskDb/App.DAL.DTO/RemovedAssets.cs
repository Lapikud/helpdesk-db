using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class RemovedAssets : IDomainId
{
    public Guid Id { get; set; }

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }

    public string Comment { get; set; } = default!;
    
    public string RemovedBy { get; set; } = default!;
}