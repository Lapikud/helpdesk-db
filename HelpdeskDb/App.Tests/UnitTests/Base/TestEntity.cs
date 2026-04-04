using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Tests.UnitTests.Base;

public class TestEntity : BaseEntity
{
    [MaxLength(128)]
    public string Value { get; set; } = default!;
}