using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class AssetMapperTest
{
    private readonly AssetMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.Asset?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.Asset?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.Asset { Id = Guid.NewGuid(), AssetName = "A", Comment = "C", SerialNumber = "sn", Barcode = "bc" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.SerialNumber, dst.SerialNumber);
        Assert.Equal(src.Barcode, dst.Barcode);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.Asset { Id = Guid.NewGuid(), AssetName = "A", Comment = "C", SerialNumber = "sn", Barcode = "bc" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetName, dst.AssetName);
        Assert.Equal(src.Comment, dst.Comment);
    }

    [Fact]
    public void Map_Create_ShouldGenerateIdAndCopyScalars()
    {
        var create = new V1.CreateObjects.AssetCreate
        {
            AssetName = "A", Comment = "C", SerialNumber = "sn", Barcode = "bc"
        };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.AssetName, dst.AssetName);
        Assert.Equal(create.Comment, dst.Comment);
        Assert.Equal(create.SerialNumber, dst.SerialNumber);
        Assert.Equal(create.Barcode, dst.Barcode);
    }
}
