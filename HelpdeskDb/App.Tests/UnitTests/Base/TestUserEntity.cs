using Base.Domain;

namespace App.Tests.UnitTests.Base;

public class TestUser
{
    public Guid Id { get; set; }
}

public class TestUserEntity : BaseEntityUser<TestUser>
{
    public string Value { get; set; } = default!;
}
