using App.Tests.UnitTests.Base;
using Base.Contracts;

namespace App.Tests.UnitTests.Base.DAL.Mappers;

public class TestUserEntityUOWMapper : IMapper<TestUserEntity, TestUserEntity>
{
    public TestUserEntity? Map(TestUserEntity? entity)
    {
        if (entity == null) return null;

        return new TestUserEntity
        {
            Id = entity.Id,
            Value = entity.Value,
            UserId = entity.UserId
        };
    }
}
