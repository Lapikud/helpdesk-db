using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class AssetReservationUOWMapperTest
{
    private readonly AssetReservationUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.AssetReservation?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.AssetReservation?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars()
    {
        var now = DateTime.UtcNow;
        var src = new Domain.AssetReservation
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ReservationFrom = now,
            ReservationTo = now.AddHours(2),
            IsReturned = true,
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.ReservationFrom, dst.ReservationFrom);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
        Assert.Equal(src.IsReturned, dst.IsReturned);
        Assert.Null(dst.Asset);
        Assert.Null(dst.User);
    }

    [Fact]
    public void Map_DomainToDal_WithNestedAssetAndUser_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var src = new Domain.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = assetId, UserId = userId,
            ReservationFrom = DateTime.UtcNow, ReservationTo = DateTime.UtcNow.AddHours(1),
            Asset = new Domain.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment", SerialNumber = "SN-123123123", Barcode = "BC-123456789",
                AssetReservations = new List<Domain.AssetReservation>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, UserId = userId,
                        ReservationFrom = DateTime.UtcNow, ReservationTo = DateTime.UtcNow.AddHours(1) }
                }
            },
            User = new Domain.Identity.AppUser { Id = userId, Username = "bob" }
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
    public void Map_DalToDomain_ShouldCopyScalars()
    {
        var now = DateTime.UtcNow;
        var src = new DalDto.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), UserId = Guid.NewGuid(),
            ReservationFrom = now, ReservationTo = now.AddHours(2), IsReturned = false,
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.UserId, dst.UserId);
        Assert.Equal(src.ReservationFrom, dst.ReservationFrom);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
        Assert.Equal(src.IsReturned, dst.IsReturned);
        Assert.Null(dst.Asset);
        Assert.Null(dst.User);
    }

    [Fact]
    public void Map_DalToDomain_WithNestedAssetAndUser_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var src = new DalDto.AssetReservation
        {
            Id = Guid.NewGuid(), AssetId = assetId, UserId = userId,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment", SerialNumber = "SN-123123123", Barcode = "BC-123456789",
                AssetReservations = new List<DalDto.AssetReservation>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, UserId = userId }
                }
            },
            User = new DalDto.Identity.AppUser { Id = userId, Username = "alice" }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.AssetReservations!);
        Assert.NotNull(dst.User);
        Assert.Equal("alice", dst.User!.Username);
    }
}
