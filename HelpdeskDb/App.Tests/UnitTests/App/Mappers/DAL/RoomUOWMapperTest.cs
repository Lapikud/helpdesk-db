using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class RoomUOWMapperTest
{
    private readonly RoomUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Room?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Room?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new Domain.Room
        {
            Id = id, RoomName = "Room", Comment = "comment",
            CupboardsInRooms = new List<Domain.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), Comment = "comment", RoomId = id, CupboardId = Guid.NewGuid() }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.All(dst.CupboardsInRooms!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Room); });
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new DalDto.Room
        {
            Id = id, RoomName = "Room", Comment = "comment",
            CupboardsInRooms = new List<DalDto.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), Comment = "comment", RoomId = id, CupboardId = Guid.NewGuid() }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.All(dst.CupboardsInRooms!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Room); });
    }
}
