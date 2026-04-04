using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class OwnerAssets : BaseEntity
{
    public Guid OwnerId { get; set; }

    public Owner? Owner { get; set; }

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }
}