using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;


namespace App.DAL.DTO;

public class Cupboard : IDomainId
{
    public Guid Id { get; set; }

    public string CodeName { get; set; } = default!;

    public ICollection<LocationInCupboard>? LocationsInCupboards { get; set; }
    public ICollection<CupboardInRoom>? CupboardsInRooms { get; set; }
}