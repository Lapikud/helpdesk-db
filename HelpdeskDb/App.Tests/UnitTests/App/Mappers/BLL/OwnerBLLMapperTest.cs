using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class OwnerBLLMapperTest
{
    private readonly OwnerBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.Owner?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Owner?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalarsAndCollection()
    {
        var id = Guid.NewGuid();
        var src = new BllDto.Owner
        {
            Id = id, OwnerName = "O", Comment = "c",
            OwnerAssets = new List<BllDto.OwnerAssets>
            {
                new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), OwnerId = id, CreatedBy = "u" }
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
    public void Map_DalToBll_ShouldCopyScalars()
    {
        var src = new DalDto.Owner { Id = Guid.NewGuid(), OwnerName = "O", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.Comment, dst.Comment);
    }
}
