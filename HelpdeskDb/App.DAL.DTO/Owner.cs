using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class Owner : IDomainId
{
    public Guid Id { get; set; }

    public string OwnerName { get; set; } = default!;

    public string Comment { get; set; } = default!;

    public ICollection<OwnerAssets>? OwnerAssets { get; set; }
}