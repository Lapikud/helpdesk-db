using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class LocationInCupboardMapperTest
{
    private readonly LocationInCupboardMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.LocationInCupboard?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.LocationInCupboard?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.LocationInCupboard { Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CupboardId, dst.CupboardId);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.LocationInCupboard { Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.LocationInCupboardCreate { LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid() };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.LocationId, dst.LocationId);
        Assert.Equal(create.CupboardId, dst.CupboardId);
    }
}
