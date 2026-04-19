using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class CategoryAssetsBLLMapperTest
{
    private readonly CategoryAssetsBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.CategoryAssets?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.CategoryAssets?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.CategoryAssets
        {
            Id = Guid.NewGuid(), Comment = "x", CreatedBy = "u",
            AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid()
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.CategoryId, dst.CategoryId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedAssetAndCategory_ShouldProjectAndNullInnerCollections()
    {
        var assetId = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var src = new DalDto.CategoryAssets
        {
            Id = Guid.NewGuid(), Comment = "cc", CreatedBy = "u",
            AssetId = assetId, CategoryId = catId,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                CategoryAssetsCollection = new List<DalDto.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = catId }
                }
            },
            Category = new DalDto.Category
            {
                Id = catId, CategoryName = "C", Comment = "cc",
                CategoryAssetsCollection = new List<DalDto.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = catId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.CategoryAssetsCollection!);
        Assert.NotNull(dst.Category);
        Assert.Equal(catId, dst.Category!.Id);
        Assert.Single(dst.Category.CategoryAssetsCollection!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNavigation()
    {
        var src = new DalDto.CategoryAssets { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid() };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Null(dst!.Asset);
        Assert.Null(dst.Category);
    }
}
