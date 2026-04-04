using Base.DAL.Contracts;

namespace App.Tests.UnitTests.Base.DAL.Contracts;

public interface ITestUOW : IBaseUOW
{
    ITestEntityRepository TestEntityRepository { get; }
}