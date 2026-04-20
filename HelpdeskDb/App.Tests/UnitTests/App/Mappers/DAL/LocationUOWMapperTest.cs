using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class LocationUOWMapperTest
{
    private readonly LocationUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Location?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Location?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new Domain.Location
        {
            Id = id, LocationName = "Location", ShelfNum = 3, Column = 5,
            LocationsInCupboards = new List<Domain.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), LocationId = id, CupboardId = Guid.NewGuid() }
            },
            LocationsAssetsCollection = new List<Domain.LocationAssets>
            {
                new() { Id = Guid.NewGuid(), LocationId = id, AssetId = Guid.NewGuid(), CreatedBy = "user" }
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
        Assert.All(dst.LocationsInCupboards!, x => { Assert.Null(x.Location); Assert.Null(x.Cupboard); });
        Assert.All(dst.LocationsAssetsCollection!, x => { Assert.Null(x.Asset); Assert.Null(x.Location); });
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new DalDto.Location
        {
            Id = id, LocationName = "Location", ShelfNum = 3, Column = 5,
            LocationsInCupboards = new List<DalDto.LocationInCupboard>
            {
                new() { Id = Guid.NewGuid(), LocationId = id, CupboardId = Guid.NewGuid() }
            },
            LocationsAssetsCollection = new List<DalDto.LocationAssets>
            {
                new() { Id = Guid.NewGuid(), LocationId = id, AssetId = Guid.NewGuid(), CreatedBy = "user" }
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
}
