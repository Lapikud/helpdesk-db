using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class CupboardInRoomUOWMapperTest
{
    private readonly CupboardInRoomUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.CupboardInRoom?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.CupboardInRoom?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new Domain.CupboardInRoom
        {
            Id = Guid.NewGuid(), Comment = "comment", CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Equal(src.RoomId, dst.RoomId);
        Assert.Null(dst.Cupboard);
        Assert.Null(dst.Room);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProjectCupboardAndRoom()
    {
        var cupboardId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var src = new Domain.CupboardInRoom
        {
            Id = Guid.NewGuid(), Comment = "comment", CupboardId = cupboardId, RoomId = roomId,
            Cupboard = new Domain.Cupboard
            {
                Id = cupboardId, CodeName = "C1",
                CupboardsInRooms = new List<Domain.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            },
            Room = new Domain.Room
            {
                Id = roomId, RoomName = "Room", Comment = "comment",
                CupboardsInRooms = new List<Domain.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Cupboard);
        Assert.Equal(cupboardId, dst.Cupboard!.Id);
        Assert.Single(dst.Cupboard.CupboardsInRooms!);
        Assert.NotNull(dst.Room);
        Assert.Equal(roomId, dst.Room!.Id);
        Assert.Single(dst.Room.CupboardsInRooms!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.CupboardInRoom
        {
            Id = Guid.NewGuid(), Comment = "comment", CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Equal(src.RoomId, dst.RoomId);
        Assert.Null(dst.Cupboard);
        Assert.Null(dst.Room);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProjectCupboardAndRoom()
    {
        var cupboardId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var src = new DalDto.CupboardInRoom
        {
            Id = Guid.NewGuid(), Comment = "comment", CupboardId = cupboardId, RoomId = roomId,
            Cupboard = new DalDto.Cupboard
            {
                Id = cupboardId, CodeName = "C1",
                CupboardsInRooms = new List<DalDto.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            },
            Room = new DalDto.Room
            {
                Id = roomId, RoomName = "Room", Comment = "comment",
                CupboardsInRooms = new List<DalDto.CupboardInRoom>
                {
                    new() { Id = Guid.NewGuid(), CupboardId = cupboardId, RoomId = roomId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Cupboard);
        Assert.Single(dst.Cupboard!.CupboardsInRooms!);
        Assert.NotNull(dst.Room);
        Assert.Single(dst.Room!.CupboardsInRooms!);
    }
}
