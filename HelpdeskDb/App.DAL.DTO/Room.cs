using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Base.Resources.Errors;

namespace App.DAL.DTO;

public class Room : IDomainId
{
    public Guid Id { get; set; }

    public string RoomName { get; set; } = default!;

    public string Comment { get; set; } = default!;

    public ICollection<CupboardInRoom>? CupboardsInRooms { get; set; }

}