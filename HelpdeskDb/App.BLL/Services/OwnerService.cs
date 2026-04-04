using App.BLL.Contracts;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;

namespace App.BLL.Services;

public class OwnerService : BaseService<App.BLL.DTO.Owner, App.DAL.DTO.Owner, App.DAL.Contracts.IOwnerRepository>,
    IOwnerService
{
    public OwnerService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.Owner, App.DAL.DTO.Owner> mapper) : base(serviceUOW, serviceUOW.OwnerRepository, mapper)
    {
    }
}