using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class LocationInCupboardBLLMapperTest
{
    private readonly LocationInCupboardBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.LocationInCupboard?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.LocationInCupboard?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CupboardId, dst.CupboardId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedLocationAndCupboard_ShouldProject()
    {
        var locId = Guid.NewGuid();
        var cupId = Guid.NewGuid();
        var src = new DalDto.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = locId, CupboardId = cupId,
            Location = new DalDto.Location
            {
                Id = locId, LocationName = "L", ShelfNum = 1, Column = 1,
                LocationsInCupboards = new List<DalDto.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locId, CupboardId = cupId }
                }
            },
            Cupboard = new DalDto.Cupboard
            {
                Id = cupId, CodeName = "C",
                LocationsInCupboards = new List<DalDto.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locId, CupboardId = cupId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(locId, dst!.Location!.Id);
        Assert.Single(dst.Location.LocationsInCupboards!);
        Assert.Equal(cupId, dst.Cupboard!.Id);
        Assert.Single(dst.Cupboard.LocationsInCupboards!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.LocationInCupboard { Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Location);
        Assert.Null(dst.Cupboard);
    }
}
