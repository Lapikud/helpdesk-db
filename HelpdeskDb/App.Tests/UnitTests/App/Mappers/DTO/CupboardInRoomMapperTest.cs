using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class CupboardInRoomMapperTest
{
    private readonly CupboardInRoomMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.CupboardInRoom?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.CupboardInRoom?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.CupboardInRoom { Id = Guid.NewGuid(), Comment = "c", CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Equal(src.RoomId, dst.RoomId);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.CupboardInRoom { Id = Guid.NewGuid(), Comment = "c", CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Equal(src.RoomId, dst.RoomId);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.CupboardInRoomCreate { Comment = "c", CupboardId = Guid.NewGuid(), RoomId = Guid.NewGuid() };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.Comment, dst.Comment);
        Assert.Equal(create.CupboardId, dst.CupboardId);
        Assert.Equal(create.RoomId, dst.RoomId);
    }
}
