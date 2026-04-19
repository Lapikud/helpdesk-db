using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class OwnerAssetsBLLMapperTest
{
    private readonly OwnerAssetsBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.OwnerAssets?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.OwnerAssets?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalars()
    {
        var src = new BllDto.OwnerAssets
        {
            Id = Guid.NewGuid(), CreatedBy = "u",
            AssetId = Guid.NewGuid(), OwnerId = Guid.NewGuid()
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.OwnerId, dst.OwnerId);
    }

    [Fact]
    public void Map_DalToBll_WithNestedAssetAndOwner_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var src = new DalDto.OwnerAssets
        {
            Id = Guid.NewGuid(), CreatedBy = "u", AssetId = assetId, OwnerId = ownerId,
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                OwnerAssets = new List<DalDto.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            },
            Owner = new DalDto.Owner
            {
                Id = ownerId, OwnerName = "O", Comment = "c",
                OwnerAssets = new List<DalDto.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(assetId, dst!.Asset!.Id);
        Assert.Single(dst.Asset.OwnerAssets!);
        Assert.Equal(ownerId, dst.Owner!.Id);
        Assert.Single(dst.Owner.OwnerAssets!);
    }

    [Fact]
    public void Map_DalToBll_WithoutNested_ShouldHaveNullNav()
    {
        var src = new DalDto.OwnerAssets { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Null(dst!.Asset);
        Assert.Null(dst.Owner);
    }
}
