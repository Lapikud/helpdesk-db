using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class CategoryAssets : IDomainId
{
    public Guid Id { get; set; }

    public string Comment { get; set; } = default!;

    public Guid CategoryId { get; set; }
    
    public Category? Category { get; set; }
    
    public Guid AssetId { get; set; }
    
    public Asset? Asset { get; set; }
    
    public string CreatedBy = "System";

}