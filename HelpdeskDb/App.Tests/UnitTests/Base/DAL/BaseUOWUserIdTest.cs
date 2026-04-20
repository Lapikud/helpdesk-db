using App.Tests.UnitTests.App;
using Microsoft.EntityFrameworkCore;

namespace App.Tests.UnitTests.Base.DAL;

[Collection("NonParallel")]
public class BaseUOWUserIdTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDbContext _ctx;
    private readonly TestUOW _testUOW;

    public BaseUOWUserIdTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _ctx = new TestDbContext(optionsBuilder.Options);
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();
        _testUOW = new TestUOW(_ctx);
    }

    // ── All / AllAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task All_WithUserId_ShouldReturnOnlyUserEntities()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await AddEntity(userA, "A1");
        await AddEntity(userA, "A2");
        await AddEntity(userB, "B1");

        var result = _testUOW.UserEntityRepository.All(userA).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(userA, e.UserId));
    }

    [Fact]
    public async Task AllAsync_WithUserId_ShouldReturnOnlyUserEntities()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        await AddEntity(userA, "A1");
        await AddEntity(userB, "B1");

        var result = (await _testUOW.UserEntityRepository.AllAsync(userA)).ToList();

        Assert.Single(result);
        Assert.Equal(userA, result[0].UserId);
    }

    // ── Find / FindAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task Find_WithUserId_ShouldReturnEntity_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "hello");

        var result = _testUOW.UserEntityRepository.Find(entity.Id, userId);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Find_WithUserId_ShouldReturnNull_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "hello");

        var result = _testUOW.UserEntityRepository.Find(entity.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task FindAsync_WithUserId_ShouldReturnEntity_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "hello");

        var result = await _testUOW.UserEntityRepository.FindAsync(entity.Id, userId);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task FindAsync_WithUserId_ShouldReturnNull_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "hello");

        var result = await _testUOW.UserEntityRepository.FindAsync(entity.Id, Guid.NewGuid());

        Assert.Null(result);
    }

    // ── Add / AddAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task Add_WithUserId_ShouldAssignUserId()
    {
        var userId = Guid.NewGuid();
        var entity = new TestUserEntity { Value = "test" };

        var added = _testUOW.UserEntityRepository.Add(entity, userId);
        await _testUOW.SaveChangesAsync();

        var stored = await _testUOW.UserEntityRepository.FindAsync(added.Id);
        Assert.NotNull(stored);
        Assert.Equal(userId, stored.UserId);
    }

    [Fact]
    public async Task AddAsync_WithUserId_ShouldAssignUserId()
    {
        var userId = Guid.NewGuid();
        var entity = new TestUserEntity { Value = "test" };

        var added = await _testUOW.UserEntityRepository.AddAsync(entity, userId);
        await _testUOW.SaveChangesAsync();

        var stored = await _testUOW.UserEntityRepository.FindAsync(added.Id);
        Assert.NotNull(stored);
        Assert.Equal(userId, stored.UserId);
    }

    // ── Update / UpdateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Update_WithUserId_ShouldUpdateEntity_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "original");
        _ctx.ChangeTracker.Clear();

        entity.Value = "updated";
        var result = _testUOW.UserEntityRepository.Update(entity, userId);
        await _testUOW.SaveChangesAsync();

        Assert.NotNull(result);
        var stored = await _testUOW.UserEntityRepository.FindAsync(entity.Id);
        Assert.Equal("updated", stored!.Value);
    }

    [Fact]
    public async Task Update_WithUserId_ShouldReturnNull_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "original");
        _ctx.ChangeTracker.Clear();

        entity.Value = "updated";
        var result = _testUOW.UserEntityRepository.Update(entity, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void Update_WithUserId_ShouldReturnNull_WhenEntityNotFound()
    {
        var entity = new TestUserEntity { Id = Guid.NewGuid(), Value = "ghost", UserId = Guid.NewGuid() };

        var result = _testUOW.UserEntityRepository.Update(entity, entity.UserId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithUserId_ShouldUpdateEntity_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "original");
        _ctx.ChangeTracker.Clear();

        entity.Value = "updated";
        var result = await _testUOW.UserEntityRepository.UpdateAsync(entity, userId);
        await _testUOW.SaveChangesAsync();

        Assert.NotNull(result);
        var stored = await _testUOW.UserEntityRepository.FindAsync(entity.Id);
        Assert.Equal("updated", stored!.Value);
    }

    [Fact]
    public async Task UpdateAsync_WithUserId_ShouldReturnNull_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "original");
        _ctx.ChangeTracker.Clear();

        entity.Value = "updated";
        var result = await _testUOW.UserEntityRepository.UpdateAsync(entity, Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithUserId_ShouldReturnNull_WhenEntityNotFound()
    {
        var entity = new TestUserEntity { Id = Guid.NewGuid(), Value = "ghost", UserId = Guid.NewGuid() };

        var result = await _testUOW.UserEntityRepository.UpdateAsync(entity, entity.UserId);

        Assert.Null(result);
    }

    // ── Remove / RemoveAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Remove_WithUserId_ShouldRemoveOwnEntity()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "mine");

        _testUOW.UserEntityRepository.Remove(entity.Id, userId);
        await _testUOW.SaveChangesAsync();

        Assert.False(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id));
    }

    [Fact]
    public async Task Remove_WithUserId_ShouldNotRemoveOtherUserEntity()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var entity = await AddEntity(userA, "userA entity");
        await AddEntity(userB, "userB entity");

        _testUOW.UserEntityRepository.Remove(entity.Id, userB);
        await _testUOW.SaveChangesAsync();

        Assert.True(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id));
    }

    [Fact]
    public async Task RemoveAsync_WithUserId_ShouldRemoveOwnEntity()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "mine");

        await _testUOW.UserEntityRepository.RemoveAsync(entity.Id, userId);
        await _testUOW.SaveChangesAsync();

        Assert.False(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id));
    }

    [Fact]
    public async Task RemoveAsync_WithUserId_ShouldNotRemoveOtherUserEntity()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var entity = await AddEntity(userA, "userA entity");
        await AddEntity(userB, "userB entity");

        await _testUOW.UserEntityRepository.RemoveAsync(entity.Id, userB);
        await _testUOW.SaveChangesAsync();

        Assert.True(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id));
    }

    // ── Exists / ExistsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task Exists_WithUserId_ShouldReturnTrue_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "x");

        Assert.True(_testUOW.UserEntityRepository.Exists(entity.Id, userId));
    }

    [Fact]
    public async Task Exists_WithUserId_ShouldReturnFalse_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "x");

        Assert.False(_testUOW.UserEntityRepository.Exists(entity.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task ExistsAsync_WithUserId_ShouldReturnTrue_WhenUserMatches()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "x");

        Assert.True(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id, userId));
    }

    [Fact]
    public async Task ExistsAsync_WithUserId_ShouldReturnFalse_WhenUserDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var entity = await AddEntity(userId, "x");

        Assert.False(await _testUOW.UserEntityRepository.ExistsAsync(entity.Id, Guid.NewGuid()));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<TestUserEntity> AddEntity(Guid userId, string value)
    {
        var entity = new TestUserEntity { Value = value, UserId = userId };
        _ctx.TestUserEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        return entity;
    }
}
