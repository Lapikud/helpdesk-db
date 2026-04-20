using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class CategoryAssetsUOWMapperTest
{
    private readonly CategoryAssetsUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.CategoryAssets?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.CategoryAssets?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.CategoryId, dst.CategoryId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Category);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProjectAssetAndCategory()
    {
        var assetId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var src = new Domain.CategoryAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId, CreatedBy = "user",
            Asset = new Domain.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment",
                CategoryAssetsCollection = new List<Domain.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId }
                }
            },
            Category = new Domain.Category
            {
                Id = categoryId, CategoryName = "Category", Comment = "comment",
                CategoryAssetsCollection = new List<Domain.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.CategoryAssetsCollection!);
        Assert.NotNull(dst.Category);
        Assert.Equal(categoryId, dst.Category!.Id);
        Assert.Single(dst.Category.CategoryAssetsCollection!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.CategoryAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.CategoryId, dst.CategoryId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Category);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProjectAssetAndCategory()
    {
        var assetId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var src = new DalDto.CategoryAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment",
                CategoryAssetsCollection = new List<DalDto.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId }
                }
            },
            Category = new DalDto.Category
            {
                Id = categoryId, CategoryName = "Category", Comment = "comment",
                CategoryAssetsCollection = new List<DalDto.CategoryAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, CategoryId = categoryId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.CategoryAssetsCollection!);
        Assert.NotNull(dst.Category);
        Assert.Single(dst.Category!.CategoryAssetsCollection!);
    }
}
