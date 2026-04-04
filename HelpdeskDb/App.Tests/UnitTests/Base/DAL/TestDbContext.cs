using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.Base.DAL;

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; } = default!;
    
    public TestDbContext(DbContextOptions options): base(options)
    {
    }
}