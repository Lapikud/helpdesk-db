using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class RemovedAssetsMapperTest
{
    private readonly RemovedAssetsMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.RemovedAssets?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.RemovedAssets?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.RemovedAssets { Id = Guid.NewGuid(), Comment = "c", AssetId = Guid.NewGuid(), RemovedBy = "u" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.RemovedBy, dst.RemovedBy);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.RemovedAssets { Id = Guid.NewGuid(), Comment = "c", AssetId = Guid.NewGuid(), RemovedBy = "u" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
    }

    [Fact]
    public void Map_Create_ShouldGenerateIdAndNotCarryRemovedBy()
    {
        var create = new V1.CreateObjects.RemovedAssetsCreate { Comment = "c", AssetId = Guid.NewGuid() };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.Comment, dst.Comment);
        Assert.Equal(create.AssetId, dst.AssetId);
    }
}
