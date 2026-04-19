using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class LocationMapperTest
{
    private readonly LocationMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.Location?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.Location?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.Location { Id = Guid.NewGuid(), LocationName = "L", ShelfNum = 1, Column = 2 };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationName, dst.LocationName);
        Assert.Equal(src.ShelfNum, dst.ShelfNum);
        Assert.Equal(src.Column, dst.Column);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.Location { Id = Guid.NewGuid(), LocationName = "L", ShelfNum = 1, Column = 2 };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationName, dst.LocationName);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.LocationCreate { LocationName = "L", ShelfNum = 1, Column = 2 };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.LocationName, dst.LocationName);
        Assert.Equal(create.ShelfNum, dst.ShelfNum);
        Assert.Equal(create.Column, dst.Column);
    }
}
