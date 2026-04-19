using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class CategoryAssetsMapperTest
{
    private readonly CategoryAssetsMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.CategoryAssets?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.CategoryAssets?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.CategoryAssets { Id = Guid.NewGuid(), Comment = "c", CreatedBy = "u", AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.CategoryId, dst.CategoryId);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.CategoryAssets { Id = Guid.NewGuid(), Comment = "c", CreatedBy = "u", AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.CategoryId, dst.CategoryId);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.CategoryAssetsCreate
        {
            Comment = "c", CreatedBy = "u", AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid()
        };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.Comment, dst.Comment);
        Assert.Equal(create.CreatedBy, dst.CreatedBy);
        Assert.Equal(create.AssetId, dst.AssetId);
        Assert.Equal(create.CategoryId, dst.CategoryId);
    }
}
