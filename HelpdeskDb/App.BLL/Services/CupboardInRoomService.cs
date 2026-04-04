using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.BLL.Services;

public class CupboardInRoomService: BaseService<App.BLL.DTO.CupboardInRoom, App.DAL.DTO.CupboardInRoom, App.DAL.Contracts.ICupboardInRoomRepository>,
    ICupboardInRoomService
{
    public CupboardInRoomService(
        IAppUOW serviceUOW,
        IMapper<CupboardInRoom, DAL.DTO.CupboardInRoom> mapper) : base(serviceUOW, serviceUOW.CupboardInRoomRepository, mapper)
    {
    }

    public async Task<CupboardInRoom?> GetCupboardInRoomByCupboardId(Guid cupboardId)
    {
        var cupboardInRoom = await ServiceRepository.GetCupboardInRoomByCupboardId(cupboardId);
        return Mapper.Map(cupboardInRoom);
    }
}