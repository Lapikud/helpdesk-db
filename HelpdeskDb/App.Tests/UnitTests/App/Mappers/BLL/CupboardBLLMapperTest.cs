using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class CupboardBLLMapperTest
{
    private readonly CupboardBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.Cupboard?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Cupboard?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new BllDto.Cupboard
        {
            Id = id,
            CodeName = "C1",
            CupboardsInRooms = new List<BllDto.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), CupboardId = id, RoomId = Guid.NewGuid(), Comment = "x" }
            },
            LocationsInCupboards = new List<BllDto.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = id }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CodeName, dst.CodeName);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.Single(dst.LocationsInCupboards!);
        Assert.All(dst.CupboardsInRooms!, cir => { Assert.Null(cir.Cupboard); Assert.Null(cir.Room); });
        Assert.All(dst.LocationsInCupboards!, lic => { Assert.Null(lic.Cupboard); Assert.Null(lic.Location); });
    }

    [Fact]
    public void Map_DalToBll_ShouldCopyScalars()
    {
        var src = new DalDto.Cupboard { Id = Guid.NewGuid(), CodeName = "X" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CodeName, dst.CodeName);
    }
}
