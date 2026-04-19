using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class LocationBLLMapperTest
{
    private readonly LocationBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.Location?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Location?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new BllDto.Location
        {
            Id = id, LocationName = "L", ShelfNum = 2, Column = 3,
            LocationsInCupboards = new List<BllDto.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), LocationId = id, CupboardId = Guid.NewGuid() }
            },
            LocationsAssetsCollection = new List<BllDto.LocationAssets>
            {
                new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), LocationId = id, CreatedBy = "u" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationName, dst.LocationName);
        Assert.Equal(src.ShelfNum, dst.ShelfNum);
        Assert.Equal(src.Column, dst.Column);
        Assert.Single(dst.LocationsInCupboards!);
        Assert.Single(dst.LocationsAssetsCollection!);
    }

    [Fact]
    public void Map_DalToBll_ShouldCopyScalars()
    {
        var src = new DalDto.Location { Id = Guid.NewGuid(), LocationName = "L", ShelfNum = 1, Column = 1 };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationName, dst.LocationName);
        Assert.Equal(src.ShelfNum, dst.ShelfNum);
        Assert.Equal(src.Column, dst.Column);
    }
}
