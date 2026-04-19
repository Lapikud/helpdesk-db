using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class AssetReservationMapperTest
{
    private readonly AssetReservationMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.AssetReservation?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.AssetReservation?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var now = DateTime.UtcNow;
        var src = new Bll.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), UserId = Guid.NewGuid(),
            ReservationFrom = now, ReservationTo = now.AddHours(1), IsReturned = true
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.ReservationFrom, dst.ReservationFrom);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
        Assert.Equal(src.IsReturned, dst.IsReturned);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var now = DateTime.UtcNow;
        var src = new V1.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), UserId = Guid.NewGuid(),
            ReservationFrom = now, ReservationTo = now.AddHours(1), IsReturned = true
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.IsReturned, dst.IsReturned);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.AssetReservationCreate
        {
            AssetId = Guid.NewGuid(), UserId = Guid.NewGuid(),
            ReservationFrom = DateTime.UtcNow, ReservationTo = DateTime.UtcNow.AddHours(1)
        };
        var dst = _mapper.Map(create);

        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.AssetId, dst.AssetId);
        Assert.Equal(create.UserId, dst.UserId);
        Assert.Equal(create.ReservationFrom, dst.ReservationFrom);
        Assert.Equal(create.ReservationTo, dst.ReservationTo);
    }
}
