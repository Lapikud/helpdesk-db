using Base.DAL.Contracts;

namespace App.DAL.Contracts;

public interface ICupboardInRoomRepository : IBaseRepository<App.DAL.DTO.CupboardInRoom>
{
    Task<App.DAL.DTO.CupboardInRoom?> GetCupboardInRoomByCupboardId(Guid cupboardId);
}