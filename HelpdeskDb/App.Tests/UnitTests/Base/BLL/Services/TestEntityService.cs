using App.Tests.UnitTests.Base.BLL.Contracts;
using App.Tests.UnitTests.Base.DAL;
using App.Tests.UnitTests.Base.DAL.Contracts;
using Base.BLL;
using Base.Contracts;
using Base.DAL.Contracts;

namespace App.Tests.UnitTests.Base.BLL.Services;

public class TestEntityService : BaseService<TestEntity, TestEntity, ITestEntityRepository>, ITestEntityService
{
    public TestEntityService(
        ITestUOW serviceUOW,
        IMapper<TestEntity, TestEntity> mapper) : base(serviceUOW, serviceUOW.TestEntityRepository, mapper)
    {
    }
}