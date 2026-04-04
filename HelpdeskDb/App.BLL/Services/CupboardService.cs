using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.BLL.Services;

public class CupboardService: BaseService<App.BLL.DTO.Cupboard, App.DAL.DTO.Cupboard, App.DAL.Contracts.ICupboardRepository>,
    ICupboardService
{
    public CupboardService(
        IAppUOW serviceUOW,
        IMapper<Cupboard, DAL.DTO.Cupboard> mapper) : base(serviceUOW, serviceUOW.CupboardRepository, mapper)
    {
    }
}