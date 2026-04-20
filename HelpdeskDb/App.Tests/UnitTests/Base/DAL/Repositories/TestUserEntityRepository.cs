using App.Tests.UnitTests.Base;
using App.Tests.UnitTests.Base.DAL.Contracts;
using App.Tests.UnitTests.Base.DAL.Mappers;
using Base.DAL.EF;

namespace App.Tests.UnitTests.Base.DAL.Repositories;

public class TestUserEntityRepository : BaseRepository<TestUserEntity, TestUserEntity>, ITestUserEntityRepository
{
    public TestUserEntityRepository(TestDbContext repositoryDbContext)
        : base(repositoryDbContext, new TestUserEntityUOWMapper())
    {
    }
}
