using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class CupboardInRoomBLLMapperTest
{
    private readonly CupboardInRoomBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.CupboardInRoom?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.CupboardInRoom?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.CupboardInRoom
        {
            Id = Guid.NewGuid(),
            Comment = "c",
            CupboardId = Guid.NewGuid(),
            RoomId = Guid.NewGuid()
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Equal(src.RoomId, dst.RoomId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedCupboardAndRoom_ShouldProject()
    {
        var cupboardId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var src = new DalDto.CupboardInRoom
        {
            Id = Guid.NewGuid(),
            Comment = "c",
            CupboardId = cupboardId,
            RoomId = roomId,
            Cupboard = new DalDto.Cupboard
            {
                Id = cupboardId, CodeName = "CC",
                CupboardsInRooms = new List<DalDto.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            },
            Room = new DalDto.Room
            {
                Id = roomId, RoomName = "RR", Comment = "x",
                CupboardsInRooms = new List<DalDto.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.Cupboard);
        Assert.Equal(cupboardId, dst.Cupboard!.Id);
        Assert.Single(dst.Cupboard.CupboardsInRooms!);
        Assert.NotNull(dst.Room);
        Assert.Equal(roomId, dst.Room!.Id);
        Assert.Single(dst.Room.CupboardsInRooms!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.CupboardInRoom { Id = Guid.NewGuid(), CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Cupboard);
        Assert.Null(dst.Room);
    }
}
