using App.BLL.Contracts;
using App.BLL.DTO;
using App.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.BLL.Services;

public class CategoryService : BaseService<App.BLL.DTO.Category, App.DAL.DTO.Category, App.DAL.Contracts.ICategoryRepository>,
    ICategoryService
{
    public CategoryService(
        IAppUOW serviceUOW,
        IMapper<Category, DAL.DTO.Category> mapper) : base(serviceUOW, serviceUOW.CategoryRepository, mapper)
    {
    }
}