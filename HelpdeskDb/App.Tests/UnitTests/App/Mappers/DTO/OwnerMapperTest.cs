using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class OwnerMapperTest
{
    private readonly OwnerMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.Owner?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.Owner?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.Owner { Id = Guid.NewGuid(), OwnerName = "O", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.OwnerName, dst.OwnerName);
        Assert.Equal(src.Comment, dst.Comment);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.Owner { Id = Guid.NewGuid(), OwnerName = "O", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.OwnerCreate { OwnerName = "O", Comment = "c" };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.OwnerName, dst.OwnerName);
        Assert.Equal(create.Comment, dst.Comment);
    }
}
