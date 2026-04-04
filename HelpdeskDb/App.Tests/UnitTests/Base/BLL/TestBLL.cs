using App.Tests.UnitTests.Base.BLL.Contracts;
using App.Tests.UnitTests.Base.BLL.Mappers;
using App.Tests.UnitTests.Base.BLL.Services;
using App.Tests.UnitTests.Base.DAL.Contracts;
using App.Tests.UnitTests.Base.DAL.Mappers;
using Base.BLL;

namespace App.Tests.UnitTests.Base.BLL;

public class TestBLL: BaseBLL<ITestUOW>, ITestBLL
{
    public TestBLL(ITestUOW uow) : base(uow)
    {
    }

    private ITestEntityService? _testEntityService;

    public ITestEntityService TestEntityService =>
        _testEntityService ??= new TestEntityService(
            BLLUOW,
            new TestEntityBLLMapper()
        );
}