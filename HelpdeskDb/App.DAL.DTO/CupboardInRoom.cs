using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class CupboardInRoom : IDomainId
{
    public Guid Id { get; set; }

    public string Comment { get; set; } = default!;

    public Guid CupboardId { get; set; }
    
    public Cupboard? Cupboard { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }
}