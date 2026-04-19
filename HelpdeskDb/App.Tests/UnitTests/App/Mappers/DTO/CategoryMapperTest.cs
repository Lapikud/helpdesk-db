using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class CategoryMapperTest
{
    private readonly CategoryMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.Category?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.Category?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.Category { Id = Guid.NewGuid(), CategoryName = "n", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.Category { Id = Guid.NewGuid(), CategoryName = "n", Comment = "c" };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
    }

    [Fact]
    public void Map_Create_ShouldGenerateIdAndCopyScalars()
    {
        var create = new V1.CreateObjects.CategoryCreate { CategoryName = "n", Comment = "c" };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.CategoryName, dst.CategoryName);
        Assert.Equal(create.Comment, dst.Comment);
    }
}
