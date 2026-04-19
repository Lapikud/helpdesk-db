using App.BLL.Mappers;
using BllVm = App.BLL.DTO.ViewModels;
using DalVm = App.DAL.DTO.ViewModels;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class AssetViewModelBLLMapperTest
{
    private readonly AssetViewModelMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllVm.AssetViewModel?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalVm.AssetViewModel?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyAllScalars()
    {
        var src = new BllVm.AssetViewModel
        {
            Id = Guid.NewGuid(),
            AssetName = "A",
            SerialNumber = "sn",
            Barcode = "bc",
            CategoryName = "cat",
            OwnerName = "o",
            RoomName = "r",
            CupboardName = "cb",
            Column = 1,
            ShelfNum = 2,
            ClosestReservationBy = "bob",
            AddedAt = DateTime.UtcNow,
            Reserved = true,
            ReservationId = Guid.NewGuid(),
            ReservationTo = DateTime.UtcNow.AddHours(1),
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.SerialNumber, dst.SerialNumber);
        Assert.Equal(src.Barcode, dst.Barcode);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.CupboardName, dst.CupboardName);
        Assert.Equal(src.Column, dst.Column);
        Assert.Equal(src.ShelfNum, dst.ShelfNum);
        Assert.Equal(src.ClosestReservationBy, dst.ClosestReservationBy);
        Assert.Equal(src.AddedAt, dst.AddedAt);
        Assert.Equal(src.Reserved, dst.Reserved);
        Assert.Equal(src.ReservationId, dst.ReservationId);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
    }

    [Fact]
    public void Map_DalToBll_ShouldCopyAllScalars()
    {
        var src = new DalVm.AssetViewModel
        {
            Id = Guid.NewGuid(),
            AssetName = "A",
            SerialNumber = "sn",
            Barcode = "bc",
            CategoryName = "cat",
            OwnerName = "o",
            RoomName = "r",
            CupboardName = "cb",
            Column = 1,
            ShelfNum = 2,
            ClosestReservationBy = "bob",
            AddedAt = DateTime.UtcNow,
            Reserved = true,
            ReservationId = Guid.NewGuid(),
            ReservationTo = DateTime.UtcNow.AddHours(1),
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.SerialNumber, dst.SerialNumber);
        Assert.Equal(src.Barcode, dst.Barcode);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.RoomName, dst.RoomName);
        Assert.Equal(src.CupboardName, dst.CupboardName);
        Assert.Equal(src.Column, dst.Column);
        Assert.Equal(src.ShelfNum, dst.ShelfNum);
        Assert.Equal(src.ClosestReservationBy, dst.ClosestReservationBy);
        Assert.Equal(src.AddedAt, dst.AddedAt);
        Assert.Equal(src.Reserved, dst.Reserved);
        Assert.Equal(src.ReservationId, dst.ReservationId);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
    }
}
