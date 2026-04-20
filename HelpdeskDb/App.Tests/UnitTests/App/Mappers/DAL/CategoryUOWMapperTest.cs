using App.DAL.EF.Mappers;
using DalDto = App.DAL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DAL;

public class CategoryUOWMapperTest
{
    private readonly CategoryUOWMapper _mapper = new();

    [Fact]
    public void Map_DomainToDal_NullReturnsNull() => Assert.Null(_mapper.Map((Domain.Category?)null));

    [Fact]
    public void Map_DalToDomain_NullReturnsNull() => Assert.Null(_mapper.Map((DalDto.Category?)null));

    [Fact]
    public void Map_DomainToDal_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new Domain.Category
        {
            Id = id, CategoryName = "Category", Comment = "comment",
            CategoryAssetsCollection = new List<Domain.CategoryAssets>
            {
                new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = id, CreatedBy = "user" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CategoryAssetsCollection!);
        Assert.All(dst.CategoryAssetsCollection!, ca => Assert.Null(ca.Asset));
        Assert.All(dst.CategoryAssetsCollection!, ca => Assert.Null(ca.Category));
    }

    [Fact]
    public void Map_DalToDomain_ShouldCopyScalarsAndCollections()
    {
        var id = Guid.NewGuid();
        var src = new DalDto.Category
        {
            Id = id, CategoryName = "Category", Comment = "comment",
            CategoryAssetsCollection = new List<DalDto.CategoryAssets>
            {
                new() { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), CategoryId = id, CreatedBy = "user" }
            }
        };

        var dst = _mapper.Map(src);

        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CategoryName, dst.CategoryName);
        Assert.Equal(src.Comment, dst.Comment);
        Assert.Single(dst.CategoryAssetsCollection!);
        Assert.All(dst.CategoryAssetsCollection!, ca => Assert.Null(ca.Asset));
        Assert.All(dst.CategoryAssetsCollection!, ca => Assert.Null(ca.Category));
    }
}
