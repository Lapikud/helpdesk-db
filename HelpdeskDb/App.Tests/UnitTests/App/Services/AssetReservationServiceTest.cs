using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.EF;

namespace App.Tests.UnitTests.App.Services;

[Collection("NonParallel")]
public class AssetReservationServiceTest : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly AppDbContext _context;
    private readonly AssetReservationService _service;

    public AssetReservationServiceTest(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        (_context, _service) = SetupDependencies();
    }

    [Fact]
    public async Task UserReserveAsset_ShouldCreateReservation()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(2);

        await _service.UserReserveAsset(TestDatabaseFixture.UserId, asset.Id, from, to);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = _context.AssetReservations
            .Single(r => r.AssetId == asset.Id && r.UserId == TestDatabaseFixture.UserId);
        Assert.Equal(from, found.ReservationFrom, TimeSpan.FromSeconds(1));
        Assert.Equal(to, found.ReservationTo, TimeSpan.FromSeconds(1));
        Assert.False(found.IsReturned);
    }

    [Fact]
    public async Task UserReserveAsset_ShouldNoOp_WhenSlotUnavailable()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(2);

        await _service.UserReserveAsset(TestDatabaseFixture.UserId, asset.Id, from, to);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Overlapping request from another user — should be rejected.
        await _service.UserReserveAsset(Guid.NewGuid(), asset.Id, from.AddMinutes(30), to.AddMinutes(30));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Single(_context.AssetReservations.Where(r => r.AssetId == asset.Id));
    }

    [Fact]
    public async Task IsAssetReservationAvailable_ReturnsFalse_WhenTimesInverted()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(-1);

        Assert.False(await _service.IsAssetReservationAvailable(asset.Id, from, to));
    }

    [Fact]
    public async Task IsAssetReservationAvailable_RespectsBuffer()
    {
        var asset = CreateAsset();
        var baseFrom = DateTime.UtcNow.AddDays(1);
        var baseTo = baseFrom.AddHours(1);
        CreateReservation(asset.Id, TestDatabaseFixture.UserId, baseFrom, baseTo);

        // Inside buffer (5 min gap) → conflict.
        var insideFrom = baseTo.AddMinutes(5);
        var insideTo = insideFrom.AddHours(1);
        Assert.False(await _service.IsAssetReservationAvailable(asset.Id, insideFrom, insideTo));

        // Outside buffer (15 min gap) → available.
        var outsideFrom = baseTo.AddMinutes(15);
        var outsideTo = outsideFrom.AddHours(1);
        Assert.True(await _service.IsAssetReservationAvailable(asset.Id, outsideFrom, outsideTo));
    }

    [Fact]
    public async Task IsAssetReservationAvailable_WithExclude_IgnoresSelf()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddDays(1);
        var to = from.AddHours(1);
        var reservation = CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        Assert.True(await _service.IsAssetReservationAvailable(
            asset.Id, from, to, reservation.Id));
    }

    [Fact]
    public async Task GetAssetReservationsByUserIdAndAssetId_ReturnsMatch()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddDays(1);
        var to = from.AddHours(1);
        var reservation = CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        var found = await _service.GetAssetReservationsByUserIdAndAssetId(
            TestDatabaseFixture.UserId, asset.Id);

        Assert.NotNull(found);
        Assert.Equal(reservation.Id, found!.Id);
    }

    [Fact]
    public async Task HasActiveOrFutureReservation_TrueForFutureUnreturned()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(1);
        CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        Assert.True(await _service.HasActiveOrFutureReservation(asset.Id, TestDatabaseFixture.UserId));
    }

    [Fact]
    public async Task HasActiveOrFutureReservation_FalseForReturned()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(1);
        CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to, isReturned: true);

        Assert.False(await _service.HasActiveOrFutureReservation(asset.Id, TestDatabaseFixture.UserId));
    }

    [Fact]
    public async Task HasActiveOrFutureReservation_FalseForPast()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(-3);
        var to = DateTime.UtcNow.AddHours(-1);
        CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        Assert.False(await _service.HasActiveOrFutureReservation(asset.Id, TestDatabaseFixture.UserId));
    }

    [Fact]
    public async Task AssetReturned_SetsIsReturnedAndTrimsReservationTo()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddMinutes(-30);
        var to = DateTime.UtcNow.AddHours(2);
        var reservation = CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        var before = DateTime.UtcNow;
        await _service.AssetReturned(TestDatabaseFixture.UserId, asset.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var updated = _context.AssetReservations.Single(r => r.Id == reservation.Id);
        Assert.True(updated.IsReturned);
        Assert.InRange(updated.ReservationTo, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public async Task AssetReturned_DoesNotTrim_WhenReservationToInPast()
    {
        var asset = CreateAsset();
        var from = DateTime.UtcNow.AddHours(-3);
        var to = DateTime.UtcNow.AddHours(-1);
        var reservation = CreateReservation(asset.Id, TestDatabaseFixture.UserId, from, to);

        await _service.AssetReturned(TestDatabaseFixture.UserId, asset.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var updated = _context.AssetReservations.Single(r => r.Id == reservation.Id);
        Assert.True(updated.IsReturned);
        Assert.Equal(to, updated.ReservationTo, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RemoveAssetReservation_RemovesActiveThenUpcoming()
    {
        var asset = CreateAsset();
        var future = CreateReservation(asset.Id, TestDatabaseFixture.UserId,
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        await _service.RemoveAssetReservation(TestDatabaseFixture.UserId, asset.Id);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Null(_context.AssetReservations.FirstOrDefault(r => r.Id == future.Id));
    }

    [Fact]
    public async Task AllAsync_IncludesAssetAndUser()
    {
        var asset = CreateAsset();
        CreateReservation(asset.Id, TestDatabaseFixture.UserId,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var results = (await _service.AllAsync()).ToList();

        Assert.NotEmpty(results);
        var r = results.First();
        Assert.NotNull(r.Asset);
        Assert.NotNull(r.User);
    }

    [Fact]
    public async Task FindAsync_IncludesAssetAndUser()
    {
        var asset = CreateAsset();
        var reservation = CreateReservation(asset.Id, TestDatabaseFixture.UserId,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = await _service.FindAsync(reservation.Id);

        Assert.NotNull(result);
        Assert.NotNull(result!.Asset);
        Assert.NotNull(result.User);
    }

    private (AppDbContext, AssetReservationService) SetupDependencies()
    {
        var context = _fixture.CreateContext();
        var uow = new AppUOW(context);
        var service = new AssetReservationService(uow, new AssetReservationBLLMapper());
        return (context, service);
    }

    private Domain.Asset CreateAsset()
    {
        var asset = new Domain.Asset
        {
            Id = Guid.NewGuid(),
            AssetName = "TestAsset",
            Comment = "Test",
        };
        _context.Assets.Add(asset);
        _context.SaveChanges();
        return asset;
    }

    private Domain.AssetReservation CreateReservation(
        Guid assetId, Guid userId, DateTime from, DateTime to, bool isReturned = false)
    {
        var reservation = new Domain.AssetReservation
        {
            Id = Guid.NewGuid(),
            AssetId = assetId,
            UserId = userId,
            ReservationFrom = from,
            ReservationTo = to,
            IsReturned = isReturned,
        };
        _context.AssetReservations.Add(reservation);
        _context.SaveChanges();
        return reservation;
    }
}