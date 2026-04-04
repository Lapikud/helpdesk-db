using App.Tests.UnitTests.Base.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.Base.BLL;

public class BaseBLLTest
{
    private readonly TestDbContext _ctx;
    private readonly TestBLL _testBLL;
    private static readonly Random Random = new();
    
    public BaseBLLTest()
    {
        // set up mock database - inmemory
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();

        // use random guid as db instance id
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _ctx = new TestDbContext(optionsBuilder.Options);

        // reset db
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();

        _testBLL = new TestBLL(new TestUOW(_ctx));
    }
    
    [Fact]
    public async Task All_ShouldReturnAllEntities()
    {
        // Arrange
        var entity1 = await AddRandomEntity();
        var entity2 = await AddRandomEntity();
        var entity3 = await AddRandomEntity();

        // Act
        var data = _testBLL.TestEntityService.All().ToList();

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
        var data = (await _testBLL.TestEntityService.AllAsync()).ToList();

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
        var data = _testBLL.TestEntityService.Find(entity.Id);

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
        var data = await _testBLL.TestEntityService.FindAsync(entity.Id);

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
        var addedEntity = _testBLL.TestEntityService.Add(entity);
        await _testBLL.SaveChangesAsync();
        var dbEntity = await _testBLL.TestEntityService.FindAsync(addedEntity.Id);

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
        var addedEntity = await _testBLL.TestEntityService.AddAsync(entity);
        await _testBLL.SaveChangesAsync();
        var dbEntity = await _testBLL.TestEntityService.FindAsync(addedEntity.Id);

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
        var updatedEntity = _testBLL.TestEntityService.Update(entity);
        await _testBLL.SaveChangesAsync();

        Assert.NotNull(updatedEntity);

        var dbEntity = await _testBLL.TestEntityService.FindAsync(updatedEntity.Id);

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
        var updatedEntity = await _testBLL.TestEntityService.UpdateAsync(entity);
        await _testBLL.SaveChangesAsync();

        Assert.NotNull(updatedEntity);

        var dbEntity = await _testBLL.TestEntityService.FindAsync(updatedEntity.Id);

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
        _testBLL.TestEntityService.Remove(entity1);
        _testBLL.TestEntityService.Remove(entity2);
        await _testBLL.SaveChangesAsync();

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
        _testBLL.TestEntityService.Remove(entity1.Id);
        _testBLL.TestEntityService.Remove(entity2.Id);
        await _testBLL.SaveChangesAsync();

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
        await _testBLL.TestEntityService.RemoveAsync(entity1.Id);
        await _testBLL.TestEntityService.RemoveAsync(entity2.Id);
        await _testBLL.SaveChangesAsync();

        // Assert
        Assert.Empty(_ctx.TestEntities);
    }

    [Fact]
    public async Task Exists_ShouldReturnTrue()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var exists = _testBLL.TestEntityService.Exists(entity.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        // Arrange
        var entity = await AddRandomEntity();

        // Act
        var exists = await _testBLL.TestEntityService.ExistsAsync(entity.Id);

        // Assert
        Assert.True(exists);
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