using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class RemovedAssetsUOWMapperTest
{
    private readonly RemovedAssetsUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.RemovedAssets?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.RemovedAssets?)null));

    [Fact]
    public void Map_DomainToDal_ShouldMapCreatedByToRemovedBy()
    {
        var src = new Domain.RemovedAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), Comment = "comment", CreatedBy = "remover"
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal("remover", dst.RemovedBy);
        Assert.Null(dst.Asset);
    }

    [Fact]
    public void Map_DomainToDal_WithNestedAsset_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var src = new Domain.RemovedAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, Comment = "comment",
            Asset = new Domain.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment",
                RemovedAssetsCollection = new List<Domain.RemovedAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Single(dst.Asset!.RemovedAssetsCollection!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldMapRemovedByToCreatedBy()
    {
        var src = new DalDto.RemovedAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), Comment = "comment", RemovedBy = "remover"
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Equal("remover", dst.CreatedBy);
        Assert.Null(dst.Asset);
    }

    [Fact]
    public void Map_DalToDomain_WithNestedAsset_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var src = new DalDto.RemovedAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, Comment = "comment",
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "Asset", Comment = "comment",
                RemovedAssetsCollection = new List<DalDto.RemovedAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Single(dst.Asset!.RemovedAssetsCollection!);
    }
}
