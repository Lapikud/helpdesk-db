using Base.Contracts;

namespace App.DTO.v1.Mappers;

public class CupboardMapper : IMapper<App.DTO.v1.Cupboard, App.BLL.DTO.Cupboard>
{
    public App.DTO.v1.Cupboard? Map(BLL.DTO.Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.DTO.v1.Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
        };
        return res;
    }

    public BLL.DTO.Cupboard? Map(App.DTO.v1.Cupboard? entity)
    {
        if (entity == null) return null;
        var res = new App.BLL.DTO.Cupboard()
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
        };
        return res;
    }
    
    public BLL.DTO.Cupboard Map(App.DTO.v1.CreateObjects.CupboardCreate entity)
    {

        var res = new App.BLL.DTO.Cupboard()
        {
            Id = Guid.NewGuid(),
            CodeName = entity.CodeName,
        };
        return res;
    }
}