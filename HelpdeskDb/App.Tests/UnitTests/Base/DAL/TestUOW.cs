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
    private ITestUserEntityRepository? _userEntityRepository;

    public ITestEntityRepository TestEntityRepository =>
        _testEntityRepository ??= new TestEntityRepository(UOWDbContext);

    public ITestUserEntityRepository UserEntityRepository =>
        _userEntityRepository ??= new TestUserEntityRepository(UOWDbContext);
}