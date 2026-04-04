using App.Tests.UnitTests.Base.DAL.Contracts;
using App.Tests.UnitTests.Base.DAL.Mappers;
using Base.Contracts;
using Base.DAL.EF;

namespace App.Tests.UnitTests.Base.DAL.Repositories;

public class TestEntityRepository : BaseRepository<TestEntity, TestEntity>, ITestEntityRepository
{
    public TestEntityRepository(TestDbContext repositoryDbContext)
        : base(repositoryDbContext, new TestEntityUOWMapper())
    {
    }
}