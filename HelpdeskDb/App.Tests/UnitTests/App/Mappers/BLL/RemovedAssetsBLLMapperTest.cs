using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class RemovedAssetsBLLMapperTest
{
    private readonly RemovedAssetsBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.RemovedAssets?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.RemovedAssets?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.RemovedAssets
        {
            Id = Guid.NewGuid(), Comment = "rm", AssetId = Guid.NewGuid(), RemovedBy = "u"
        };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.RemovedBy, dst.RemovedBy);
    }

    [Fact]
    public void Map_DalToBll_WithNestedAsset_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var src = new DalDto.RemovedAssets
        {
            Id = Guid.NewGuid(), Comment = "rm", AssetId = assetId, RemovedBy = "u",
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                RemovedAssetsCollection = new List<DalDto.RemovedAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.NotNull(dst!.Asset);
        Assert.Equal(assetId, dst.Asset!.Id);
        Assert.Single(dst.Asset.RemovedAssetsCollection!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullAsset()
    {
        var src = new DalDto.RemovedAssets { Id = Guid.NewGuid(), AssetId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Asset);
    }
}
