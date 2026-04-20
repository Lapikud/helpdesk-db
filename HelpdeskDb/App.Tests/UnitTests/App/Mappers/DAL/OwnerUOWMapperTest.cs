using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class OwnerUOWMapperTest
{
    private readonly OwnerUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Owner?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Owner?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new Domain.Owner
        {
            Id = id, OwnerName = "Owner", Comment = "comment",
            OwnerAssets = new List<Domain.OwnerAssets>
            {
                new() { Id = Guid.NewGuid(), OwnerId = id, AssetId = Guid.NewGuid(), CreatedBy = "user" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.OwnerAssets!);
        Assert.All(dst.OwnerAssets!, oa => { Assert.Null(oa.Asset); Assert.Null(oa.Owner); });
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new DalDto.Owner
        {
            Id = id, OwnerName = "Owner", Comment = "comment",
            OwnerAssets = new List<DalDto.OwnerAssets>
            {
                new() { Id = Guid.NewGuid(), OwnerId = id, AssetId = Guid.NewGuid(), CreatedBy = "user" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.OwnerAssets!);
        Assert.All(dst.OwnerAssets!, oa => { Assert.Null(oa.Asset); Assert.Null(oa.Owner); });
    }
}
