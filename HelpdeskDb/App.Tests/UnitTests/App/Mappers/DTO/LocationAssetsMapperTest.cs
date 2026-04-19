using App.DTO.v1.Mappers;
using V1 = App.DTO.v1;
using Bll = App.BLL.DTO;

namespace App.Tests.UnitTests.App.Mappers.DTO;

public class LocationAssetsMapperTest
{
    private readonly LocationAssetsMapper _mapper = new();

    [Fact]
    public void Map_BllToV1_NullReturnsNull() => Assert.Null(_mapper.Map((Bll.LocationAssets?)null));

    [Fact]
    public void Map_V1ToBll_NullReturnsNull() => Assert.Null(_mapper.Map((V1.LocationAssets?)null));

    [Fact]
    public void Map_BllToV1_ShouldCopyScalars()
    {
        var src = new Bll.LocationAssets { Id = Guid.NewGuid(), CreatedBy = "u", AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
        Assert.Equal(src.CreatedBy, dst.CreatedBy);
        Assert.Equal(src.AssetId, dst.AssetId);
        Assert.Equal(src.LocationId, dst.LocationId);
    }

    [Fact]
    public void Map_V1ToBll_ShouldCopyScalars()
    {
        var src = new V1.LocationAssets { Id = Guid.NewGuid(), CreatedBy = "u", AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid() };
        var dst = _mapper.Map(src);
        Assert.NotNull(dst);
        Assert.Equal(src.Id, dst!.Id);
    }

    [Fact]
    public void Map_Create_ShouldGenerateId()
    {
        var create = new V1.CreateObjects.LocationAssetsCreate { CreatedBy = "u", AssetId = Guid.NewGuid(), LocationId = Guid.NewGuid() };
        var dst = _mapper.Map(create);
        Assert.NotEqual(Guid.Empty, dst.Id);
        Assert.Equal(create.CreatedBy, dst.CreatedBy);
        Assert.Equal(create.AssetId, dst.AssetId);
        Assert.Equal(create.LocationId, dst.LocationId);
    }
}
