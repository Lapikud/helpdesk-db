using App.BLL.Mappers;
using BllDto = App.BLL.DTO;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.BLL;

public class CategoryBLLMapperTest
{
    private readonly CategoryBLLMapper _mapper = new();

    [Fact]
    public void Map_BllToDal_NullReturnsNull() => Assert.Null(_mapper.Map((BllDto.Category?)null));

    [Fact]
    public void Map_DalToBll_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Category?)null));

    [Fact]
    public void Map_BllToDal_ShouldCopyScalarsAndCollection()
    {
        var catId = Guid.NewGuid();
        var src = new BllDto.Category
        {
            Id = catId,
            CategoryName = "Cat",
            Comment = "cc",
            CategoryAssetsCollection = new List<BllDto.CategoryAssets>
            {
                new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = catId, Comment = "c", CreatedBy = "u" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CategoryAssetsCollection!);
        Assert.All(dst.CategoryAssetsCollection!, ca => { Assert.Null(ca.Asset); Assert.Null(ca.Category); });
    }

    [Fact]
    public void Map_DalToBll_ShouldCopyScalars()
    {
        var src = new DalDto.Category { Id = Guid.NewGuid(), CategoryName = "C", Comment = "c" };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
    }
}
