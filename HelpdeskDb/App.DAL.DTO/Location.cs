using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class Location : IDomainId
{
    public Guid Id { get; set; }

    public string LocationName { get; set; } = default!;

    public int ShelfNum { get; set; }

    public int Column { get; set; }

    public ICollection<LocationInCupboard>? LocationsInCupboards { get; set; }
    public ICollection<LocationAssets>? LocationsAssetsCollection { get; set; }
}