using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class LocationInCupboard : BaseEntity
{
    public Guid LocationId { get; set; }

    public Location? Location { get; set; }

    public Guid CupboardId { get; set; }

    public Cupboard? Cupboard { get; set; }
}