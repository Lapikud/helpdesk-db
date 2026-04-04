using Base.BLL.Contracts;

namespace App.Tests.UnitTests.Base.BLL.Contracts;

public interface ITestBLL : IBaseBLL
{
    ITestEntityService TestEntityService { get; }
}