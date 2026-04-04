using App.BLL.Contracts;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class RoomService : BaseService<App.BLL.DTO.Room, App.DAL.DTO.Room, App.DAL.Contracts.IRoomRepository>,
    IRoomService
{
    public RoomService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.Room, App.DAL.DTO.Room> mapper) : base(serviceUOW, serviceUOW.RoomRepository, mapper)
    {
    }
}