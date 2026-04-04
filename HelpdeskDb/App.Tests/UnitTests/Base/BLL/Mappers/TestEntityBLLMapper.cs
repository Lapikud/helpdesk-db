using Base.Contracts;

namespace App.Tests.UnitTests.Base.BLL.Mappers;

public class TestEntityBLLMapper: IMapper<TestEntity, TestEntity>
{
    public TestEntity? Map(TestEntity? entity)
    {
        if (entity == null) return null;

        var res = new TestEntity()
        {
            Id = entity.Id,
            Value = entity.Value
        };
        return res;
    }
}