using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class CategoryMapper : IMapper<App.DTO.v1.Category, App.BLL.DTO.Category>
{
    public App.DTO.v1.Category? Map(BLL.DTO.Category? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
        };
        return res;
    }

    public BLL.DTO.Category? Map(App.DTO.v1.Category? entity)
    {
        if (entity == null) return null;
        var res = new BLL.DTO.Category()
        {
            Id = entity.Id,
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
        };
        return res;
    }
    
    public BLL.DTO.Category Map(App.DTO.v1.CreateObjects.CategoryCreate entity)
    {

        var res = new BLL.DTO.Category()
        {
            Id = Guid.NewGuid(),
            CategoryName = entity.CategoryName,
            Comment = entity.Comment,
        };
        return res;
    }
}