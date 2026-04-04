using App.Tests.UnitTests.Base.DAL.Contracts;
using App.Tests.UnitTests.Base.DAL.Repositories;
using Base.DAL.EF;

namespace App.Tests.UnitTests.Base.DAL;

public class TestUOW: BaseUOW<TestDbContext>, ITestUOW
{
    public TestUOW(TestDbContext uowDbContext) : base(uowDbContext)
    {
    }

    private ITestEntityRepository? _testEntityRepository;

    public ITestEntityRepository TestEntityRepository =>
        _testEntityRepository ??= new TestEntityRepository(UOWDbContext);
}