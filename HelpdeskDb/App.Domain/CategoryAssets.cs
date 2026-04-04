using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Resources.Errors;

namespace App.Domain;

public class CategoryAssets : BaseEntity
{
    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }

    public Guid AssetId { get; set; }

    public Asset? Asset { get; set; }
}