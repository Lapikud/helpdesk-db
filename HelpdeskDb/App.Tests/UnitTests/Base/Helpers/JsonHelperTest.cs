using System.Text.Json;
using Base.Helpers;

namespace App.Tests.UnitTests.Base.Helpers;

public class JsonHelperTest
{
    private class Sample
    {
        public string FirstName { get; set; } = "";
        public int SomeValue { get; set; }
    }

    [Fact]
    public void CamelCase_ShouldSetCamelCasePolicy()
    {
        Assert.Equal(JsonNamingPolicy.CamelCase, JsonHelper.CamelCase.PropertyNamingPolicy);
    }

    [Fact]
    public void CamelCase_ShouldWriteIndented()
    {
        Assert.True(JsonHelper.CamelCase.WriteIndented);
    }

    [Fact]
    public void CamelCase_ShouldSerializePropertiesAsCamelCase()
    {
        var sample = new Sample { FirstName = "Ada", SomeValue = 42 };

        var json = JsonSerializer.Serialize(sample, JsonHelper.CamelCase);

        Assert.Contains("\"firstName\"", json);
        Assert.Contains("\"someValue\"", json);
        Assert.DoesNotContain("\"FirstName\"", json);
    }
}
