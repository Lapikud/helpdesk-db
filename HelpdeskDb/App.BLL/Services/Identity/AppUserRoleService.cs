using App.BLL.Contracts;
using App.BLL.Contracts.Identity;
using App.DAL.Contracts;
using App.DAL.DTO;
using Base.BLL;
using Base.BLL.Contracts;
using Base.Contracts;
using Base.DAL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts.Identity;

namespace App.BLL.Services.Identity;

public class AppUserRoleService : BaseService<App.BLL.DTO.Identity.AppUserRole, App.DAL.DTO.Identity.AppUserRole,
        IAppUserRoleRepository>,
    IAppUserRoleService
{
    public AppUserRoleService(
        IAppUOW serviceUOW,
        IMapper<App.BLL.DTO.Identity.AppUserRole, App.DAL.DTO.Identity.AppUserRole> mapper) : base(serviceUOW,
        serviceUOW.AppUserRoleRepository, mapper)
    {
    }
}