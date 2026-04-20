using App.Tests.UnitTests.App;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.Base.DAL;

[Collection("NonParallel")]
public class BaseUOWTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDbContext _ctx;
    private readonly TestUOW _testUOW;
    private static readonly Random Random = new();

    public BaseUOWTest()
    {
        // set up mock database - inmemory
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();

        // use random guid as db instance id
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _ctx = new TestDbContext(optionsBuilder.Options);

        // reset db
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();

        _testUOW = new TestUOW(_ctx);
    }

    [Fact]
    public async Task All_ShouldReturnAllEntities()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();
        var entity3 = await AddRandomEntity();

        // Act
        var data = _testUOW.TestEntityRepository.All().ToList();

        // Assert
        Assert.Equal(3, data.Count);
        Assert.Contains(data, e => e.Id == entity1.Id);
        Assert.Contains(data, e => e.Id == entity2.Id);
        Assert.Contains(data, e => e.Id == entity3.Id);
    }

    [Fact]
    public async Task AllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();
        var entity3 = await AddRandomEntity();

        // Act
        var data = (await _testUOW.TestEntityRepository.AllAsync()).ToList();

        // Assert
        Assert.Equal(3, data.Count);
        Assert.Contains(data, e => e.Id == entity1.Id);
        Assert.Contains(data, e => e.Id == entity2.Id);
        Assert.Contains(data, e => e.Id == entity3.Id);
    }

    [Fact]
    public async Task Find_ShouldReturnEntity()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var data = _testUOW.TestEntityRepository.Find(entity.Id);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(entity.Value, data.Value);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEntity()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var data = await _testUOW.TestEntityRepository.FindAsync(entity.Id);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(entity.Value, data.Value);
    }

    [Fact]
    public async Task Add_ShouldAddEntity()
    {
        // Arrange
        var entity = CreateRandomEntity();

        // Act
        var addedEntity = _testUOW.TestEntityRepository.Add(entity);
        await _testUOW.SaveChangesAsync();
        var dbEntity = await _testUOW.TestEntityRepository.FindAsync(addedEntity.Id);

        // Assert
        Assert.NotNull(addedEntity);
        Assert.NotEqual(Guid.Empty, addedEntity.Id);
        Assert.Equal(addedEntity.Value, entity.Value);

        Assert.NotNull(dbEntity);
        Assert.Equal(addedEntity.Id, dbEntity.Id);
        Assert.Equal(addedEntity.Value, dbEntity.Value);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var entity = CreateRandomEntity();

        // Act
        var addedEntity = await _testUOW.TestEntityRepository.AddAsync(entity);
        await _testUOW.SaveChangesAsync();
        var dbEntity = await _testUOW.TestEntityRepository.FindAsync(addedEntity.Id);

        // Assert
        Assert.NotNull(addedEntity);
        Assert.NotEqual(Guid.Empty, addedEntity.Id);
        Assert.Equal(addedEntity.Value, entity.Value);

        Assert.NotNull(dbEntity);
        Assert.Equal(addedEntity.Id, dbEntity.Id);
        Assert.Equal(addedEntity.Value, dbEntity.Value);
    }

    [Fact]
    public async Task Update_ShouldUpdateEntity()
    {
        // Arrange
        var entity = await AddRandomEntity();
        _ctx.ChangeTracker.Clear();

        // Act
        entity.Value = "Updated Value";
        var updatedEntity = _testUOW.TestEntityRepository.Update(entity);
        await _testUOW.SaveChangesAsync();

        Assert.NotNull(updatedEntity);

        var dbEntity = await _testUOW.TestEntityRepository.FindAsync(updatedEntity.Id);

        // Assert
        Assert.Equal(entity.Id, updatedEntity.Id);
        Assert.Equal("Updated Value", updatedEntity.Value);

        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Id, dbEntity.Id);
        Assert.Equal("Updated Value", dbEntity.Value);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var entity = await AddRandomEntity();
        _ctx.ChangeTracker.Clear();

        // Act
        entity.Value = "Updated Value";
        var updatedEntity = await _testUOW.TestEntityRepository.UpdateAsync(entity);
        await _testUOW.SaveChangesAsync();

        Assert.NotNull(updatedEntity);

        var dbEntity = await _testUOW.TestEntityRepository.FindAsync(updatedEntity.Id);

        // Assert
        Assert.Equal(entity.Id, updatedEntity.Id);
        Assert.Equal("Updated Value", updatedEntity.Value);

        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Id, dbEntity.Id);
        Assert.Equal("Updated Value", dbEntity.Value);
    }

    [Fact]
    public async Task RemoveByEntity_ShouldRemoveEntity()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();

        // Act
        _testUOW.TestEntityRepository.Remove(entity1);
        _testUOW.TestEntityRepository.Remove(entity2);
        await _testUOW.SaveChangesAsync();

        // Assert
        Assert.Empty(_ctx.TestEntities);
    }

    [Fact]
    public async Task RemoveById_ShouldRemoveEntity()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();

        // Act
        _testUOW.TestEntityRepository.Remove(entity1.Id);
        _testUOW.TestEntityRepository.Remove(entity2.Id);
        await _testUOW.SaveChangesAsync();

        // Assert
        Assert.Empty(_ctx.TestEntities);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveEntity()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();

        // Act
        await _testUOW.TestEntityRepository.RemoveAsync(entity1.Id);
        await _testUOW.TestEntityRepository.RemoveAsync(entity2.Id);
        await _testUOW.SaveChangesAsync();

        // Assert
        Assert.Empty(_ctx.TestEntities);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var exists = _testUOW.TestEntityRepository.Exists(entity.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var exists = await _testUOW.TestEntityRepository.ExistsAsync(entity.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void Find_ShouldReturnNull_WhenMissing()
    {
        var found = _testUOW.TestEntityRepository.Find(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnNull_WhenMissing()
    {
        var found = await _testUOW.TestEntityRepository.FindAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public void Exists_ShouldReturnFalse_WhenMissing()
    {
        Assert.False(_testUOW.TestEntityRepository.Exists(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenMissing()
    {
        Assert.False(await _testUOW.TestEntityRepository.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistMultipleEntities()
    {
        var e1 = CreateRandomEntity();
        var e2 = CreateRandomEntity();
        var e3 = CreateRandomEntity();

        await _testUOW.TestEntityRepository.AddAsync(e1);
        await _testUOW.TestEntityRepository.AddAsync(e2);
        await _testUOW.TestEntityRepository.AddAsync(e3);

        var changes = await _testUOW.SaveChangesAsync();

        Assert.Equal(3, changes);
        Assert.Equal(3, _ctx.TestEntities.Count());
    }

    private async Task<TestEntity> AddRandomEntity()
    {
        var entity = CreateRandomEntity();
        _ctx.TestEntities.Add(entity);
        await _ctx.SaveChangesAsync();

        return entity;
    }

    private static TestEntity CreateRandomEntity()
    {
        return new TestEntity { Value = RandomString(10) };
    }

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var result = new char[length];

        for (var i = 0; i < length; i++)
        {
            result[i] = chars[Random.Next(chars.Length)];
        }

        return new string(result);
    }
}