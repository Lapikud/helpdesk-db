using Base.BLL.Contracts;

namespace App.BLL.Contracts;

public interface ICupboardInRoomService : IBaseService<App.BLL.DTO.CupboardInRoom>
{
    Task<App.BLL.DTO.CupboardInRoom?> GetCupboardInRoomByCupboardId(Guid cupboardId);
}