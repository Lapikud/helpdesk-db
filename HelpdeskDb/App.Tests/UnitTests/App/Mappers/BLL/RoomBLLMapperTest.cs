using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class RoomBLLMapperTest
{
    private readonly RoomBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.Room?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Room?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalarsAndCollection()
    {
        var id = Guid.NewGuid();
        var src = new BllDto.Room
        {
            Id = id, RoomName = "R", Comment = "c",
            CupboardsInRooms = new List<BllDto.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), CupboardId = Guid.NewGuid(), RoomId = id, Comment = "x" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.All(dst.CupboardsInRooms!, cir => { Assert.Null(cir.Cupboard); Assert.Null(cir.Room); });
    }

    [Fact]
    public void Map_DalToBll_ShouldCopyScalars()
    {
        var src = new DalDto.Room { Id = Guid.NewGuid(), RoomName = "R", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.Comment, dst.Comment);
    }
}
