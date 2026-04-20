using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class OwnerAssetsUOWMapperTest
{
    private readonly OwnerAssetsUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.OwnerAssets?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.OwnerAssets?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalars_AndNullNavs()
    {
        var src = new Domain.OwnerAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.OwnerId, dst.OwnerId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Owner);
    }

    [Fact]
    public void Map_DomainToDal_WithNested_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var src = new Domain.OwnerAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId, CreatedBy = "user",
            Asset = new Domain.Asset
            {
                Id = assetId, AssetName = "A", Comment = "c",
                OwnerAssets = new List<Domain.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            },
            Owner = new Domain.Owner
            {
                Id = ownerId, OwnerName = "Owner", Comment = "comment",
                OwnerAssets = new List<Domain.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Single(dst.Asset!.OwnerAssets!);
        Assert.NotNull(dst.Owner);
        Assert.Single(dst.Owner!.OwnerAssets!);
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalars_AndNullNavs()
    {
        var src = new DalDto.OwnerAssets
        {
            Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CreatedBy = "user"
        };
        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.OwnerId, dst.OwnerId);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Null(dst.Asset);
        Assert.Null(dst.Owner);
    }

    [Fact]
    public void Map_DalToDomain_WithNested_ShouldProject()
    {
        var assetId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var src = new DalDto.OwnerAssets
        {
            Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId, CreatedBy = "user",
            Asset = new DalDto.Asset
            {
                Id = assetId, AssetName = "asset", Comment = "comment",
                OwnerAssets = new List<DalDto.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            },
            Owner = new DalDto.Owner
            {
                Id = ownerId, OwnerName = "Owner", Comment = "comment",
                OwnerAssets = new List<DalDto.OwnerAssets>
                {
                    new() { Id = Guid.NewGuid(), AssetId = assetId, OwnerId = ownerId }
                }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst!.Asset);
        Assert.Single(dst.Asset!.OwnerAssets!);
        Assert.NotNull(dst.Owner);
        Assert.Single(dst.Owner!.OwnerAssets!);
    }
}
