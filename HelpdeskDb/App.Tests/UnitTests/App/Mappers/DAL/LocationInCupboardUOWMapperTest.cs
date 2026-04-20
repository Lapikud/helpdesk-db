using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class LocationInCupboardUOWMapperTest
{
    private readonly LocationInCupboardUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.LocationInCupboard?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.LocationInCupboard?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new Domain.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Null(dst.Location);
        Assert.Null(dst.Cupboard);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProject()
    {
        var locationId = Guid.NewGuid();
        var cupboardId = Guid.NewGuid();
        var src = new Domain.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId,
            Location = new Domain.Location
            {
                Id = locationId, LocationName = "Location", ShelfNum = 1, Column = 2,
                LocationsInCupboards = new List<Domain.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId }
                }
            },
            Cupboard = new Domain.Cupboard
            {
                Id = cupboardId, CodeName = "Codename",
                LocationsInCupboards = new List<Domain.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Location);
        Assert.Single(dst.Location!.LocationsInCupboards!);
        Assert.NotNull(dst.Cupboard);
        Assert.Single(dst.Cupboard!.LocationsInCupboards!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = Guid.NewGuid(), CupboardId = Guid.NewGuid()
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.LocationId, dst.LocationId);
        Assert.Equal(src.CupboardId, dst.CupboardId);
        Assert.Null(dst.Location);
        Assert.Null(dst.Cupboard);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProject()
    {
        var locationId = Guid.NewGuid();
        var cupboardId = Guid.NewGuid();
        var src = new DalDto.LocationInCupboard
        {
            Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId,
            Location = new DalDto.Location
            {
                Id = locationId, LocationName = "Location", ShelfNum = 1, Column = 2,
                LocationsInCupboards = new List<DalDto.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId }
                }
            },
            Cupboard = new DalDto.Cupboard
            {
                Id = cupboardId, CodeName = "Codename",
                LocationsInCupboards = new List<DalDto.LocationInCupboard>
                {
                    new() { Id = Guid.NewGuid(), LocationId = locationId, CupboardId = cupboardId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Location);
        Assert.Single(dst.Location!.LocationsInCupboards!);
        Assert.NotNull(dst.Cupboard);
        Assert.Single(dst.Cupboard!.LocationsInCupboards!);
    }
}
