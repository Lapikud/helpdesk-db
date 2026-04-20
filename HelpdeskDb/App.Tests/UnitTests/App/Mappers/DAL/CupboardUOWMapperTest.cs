using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class CupboardUOWMapperTest
{
    private readonly CupboardUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Cupboard?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Cupboard?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new Domain.Cupboard
        {
            Id = id, CodeName = "C1",
            CupboardsInRooms = new List<Domain.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), Comment = "comment", CupboardId = id, RoomId = Guid.NewGuid() }
            },
            LocationsInCupboards = new List<Domain.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), CupboardId = id, LocationId = Guid.NewGuid() }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CodeName, dst.CodeName);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.Single(dst.LocationsInCupboards!);
        Assert.All(dst.CupboardsInRooms!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Room); });
        Assert.All(dst.LocationsInCupboards!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Location); });
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new DalDto.Cupboard
        {
            Id = id, CodeName = "C1",
            CupboardsInRooms = new List<DalDto.CupboardInRoom>
            {
                new() { Id = Guid.NewGuid(), Comment = "comment", CupboardId = id, RoomId = Guid.NewGuid() }
            },
            LocationsInCupboards = new List<DalDto.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), CupboardId = id, LocationId = Guid.NewGuid() }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CodeName, dst.CodeName);
        Assert.Single(dst.CupboardsInRooms!);
        Assert.Single(dst.LocationsInCupboards!);
        Assert.All(dst.CupboardsInRooms!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Room); });
        Assert.All(dst.LocationsInCupboards!, x => { Assert.Null(x.Cupboard); Assert.Null(x.Location); });
    }
}
