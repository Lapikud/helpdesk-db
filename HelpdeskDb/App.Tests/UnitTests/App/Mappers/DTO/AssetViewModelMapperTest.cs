using App.DTO.v1.Mappers;
using V1Vm = App.DTO.v1.ViewModels;
using BllVm = App.BLL.DTO.ViewModels;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class AssetViewModelMapperTest
{
    private readonly AssetViewModelMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((BllVm.AssetViewModel?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1Vm.AssetViewModel?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyAllScalars()
    {
        var src = new BllVm.AssetViewModel
        {
            Id = Guid.NewGuid(), AssetName = "A", SerialNumber = "s", Barcode = "b",
            CategoryName = "c", OwnerName = "o", RoomName = "r", CupboardName = "cb",
            Column = 1, ShelfNum = 2, ClosestReservationBy = "bob",
            AddedAt = DateTime.UtcNow, Reserved = true,
            ReservationId = Guid.NewGuid(), ReservationTo = DateTime.UtcNow.AddHours(1)
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.Reserved, dst.Reserved);
        Assert.Equal(src.ReservationTo, dst.ReservationTo);
        Assert.Equal(src.ReservationId, dst.ReservationId);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyAllScalars()
    {
        var src = new V1Vm.AssetViewModel
        {
            Id = Guid.NewGuid(), AssetName = "A",
            CategoryName = "c", OwnerName = "o", RoomName = "r", CupboardName = "cb",
            ClosestReservationBy = "bob", AddedAt = DateTime.UtcNow
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.CategoryName, dst.CategoryName);
    }
}
