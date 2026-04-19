using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class AssetReservationBLLMapperTest
{
    private readonly AssetReservationBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.AssetReservation?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.AssetReservation?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var now = DateTime.UtcNow;
        var src = new BllDto.AssetReservation
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ReservationFrom = now,
            ReservationTo = now.AddHours(2),
            IsReturned = true
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
    public void Map_DalToBll_WithNestedAssetAndUser_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var src = new DalDto.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = assetId, UserId = userId,
            ReservationFrom = DateTime.UtcNow, ReservationTo = DateTime.UtcNow.AddHours(1),
            IsReturned = false,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c", SerialNumber = "s", Barcode = "b",
                AssetReservations = new List<DalDto.AssetReservation>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, UserId = userId,
                        ReservationFrom = DateTime.UtcNow, ReservationTo = DateTime.UtcNow.AddHours(1) }
                }
            },
            User = new DalDto.Identity.AppUser { Id = userId, Username = "bob" }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.AssetReservations!);
        Assert.NotNull(dst.User);
        Assert.Equal(userId, dst.User!.Id);
        Assert.Equal("bob", dst.User.Username);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.AssetReservation { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Asset);
        Assert.Null(dst.User);
    }
}
