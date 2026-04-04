using Base.Contracts;

namespace App.Tests.UnitTests.Base.DAL.Mappers;

public class TestEntityUOWMapper: IMapper<TestEntity, TestEntity>
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