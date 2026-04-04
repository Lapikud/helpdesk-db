using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DAL.DTO;

public class OwnerAssets : IDomainId
{   
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }
    
    public Owner? Owner { get; set; }

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }
    
    public string CreatedBy = "System";
}