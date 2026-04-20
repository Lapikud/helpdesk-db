using App.DAL.EF;
using App.Domain;
using Base.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace App.Tests.UnitTests.App;

[Collection("NonParallel")]
public class AppDbContextTest : IClassFixture<TestDatabaseFixture>
{
    private const string TestUser = "TestUser";

    private static (AppDbContext, Mock<ILogger<AppDbContext>>) CreateContext(string? username = TestUser)
    {
        var userNameResolverMock = new Mock<IUserNameResolver>();
        userNameResolverMock.Setup(x => x.CurrentUserName).Returns(username!);
        var loggerMock = new Mock<ILogger<AppDbContext>>();

        var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AppDbContextTest_{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options,
            userNameResolverMock.Object,
            loggerMock.Object);

        context.Database.EnsureCreated();
        return (context, loggerMock);
    }

    [Fact]
    public async Task SaveChangesAsync_OnAdd_StampsCreatedAtAndCreatedBy()
    {
        var ct = TestContext.Current.CancellationToken;
        var (context, _) = CreateContext();
        var before = DateTime.UtcNow;

        var category = new Category { Id = Guid.NewGuid(), CategoryName = "Category", Comment = "comment" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        var after = DateTime.UtcNow;
        Assert.InRange(category.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(TestUser, category.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_OnUpdate_StampsChangedAndPreservesCreated()
    {
        var ct = TestContext.Current.CancellationToken;
        var (context, _) = CreateContext();

        var category = new Category { Id = Guid.NewGuid(), CategoryName = "Old", Comment = "c" };
        context.Categories.Add(category);
        await context.SaveChangesAsync(ct);

        var originalCreatedAt = category.CreatedAt;
        var originalCreatedBy = category.CreatedBy;

        // Simulate a delay so ChangedAt differs from CreatedAt
        await Task.Delay(10, ct);

        category.CategoryName = "New";
        // Try to overwrite CreatedBy — should be ignored by SaveChangesAsync
        category.CreatedBy = "impostor";
        category.CreatedAt = DateTime.UtcNow.AddYears(-5);
        
        var beforeSave  = DateTime.UtcNow;
        
        await context.SaveChangesAsync(ct);
        
        var afterSave = DateTime.UtcNow;

        context.ChangeTracker.Clear();
        var reloaded = await context.Categories.SingleAsync(c => c.Id == category.Id, ct);

        Assert.Equal("New", reloaded.CategoryName);
        Assert.Equal(originalCreatedAt, reloaded.CreatedAt);
        Assert.Equal(originalCreatedBy, reloaded.CreatedBy);
        Assert.Equal(TestUser, reloaded.ChangedBy);
        Assert.InRange(reloaded.ChangedAt, beforeSave.AddSeconds(-1), afterSave.AddSeconds(1));
    }

    [Fact]
    public async Task SaveChangesAsync_OnIDomainUserIdUpdate_BlocksUserIdChange_AndLogsWarning()
    {
        var ct = TestContext.Current.CancellationToken;
        var (context, loggerMock) = CreateContext();

        var user = new Domain.Identity.AppUser { Id = Guid.NewGuid(), Username = "user" };
        var asset = new Asset { Id = Guid.NewGuid(), AssetName = "Asset", Comment = "comment" };
        context.Users.Add(user);
        context.Assets.Add(asset);
        await context.SaveChangesAsync(ct);

        var originalUserId = user.Id;
        var reservation = new AssetReservation
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            UserId = originalUserId,
            ReservationFrom = DateTime.UtcNow,
            ReservationTo = DateTime.UtcNow.AddHours(1),
            IsReturned = false,
        };
        context.AssetReservations.Add(reservation);
        await context.SaveChangesAsync(ct);

        context.ChangeTracker.Clear();
        var tracked = await context.AssetReservations.SingleAsync(r => r.Id == reservation.Id, ct);
        tracked.UserId = Guid.NewGuid(); // forbidden mutation
        tracked.IsReturned = true;        // allowed mutation
        await context.SaveChangesAsync(ct);

        context.ChangeTracker.Clear();
        var reloaded = await context.AssetReservations.SingleAsync(r => r.Id == reservation.Id, ct);
        Assert.Equal(originalUserId, reloaded.UserId);
        Assert.True(reloaded.IsReturned);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
