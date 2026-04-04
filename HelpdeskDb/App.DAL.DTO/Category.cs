using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class Category: IDomainId
{
    public Guid Id { get; set; }

    public string CategoryName { get; set; } = default!;

    public string Comment { get; set; } = default!;

    public ICollection<CategoryAssets>? CategoryAssetsCollection { get; set; }
}