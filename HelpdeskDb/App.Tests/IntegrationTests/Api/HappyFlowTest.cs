using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using App.DAL.EF;
using App.DTO.v1;
using App.DTO.v1.CreateObjects;
using App.DTO.v1.Identity;
using App.DTO.v1.UpdateObjects;
using App.DTO.v1.ViewModels;
using Base.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.IntegrationTests.Api;

[Collection("NonParallel")]
public class HappyFlowTest : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private const string LoginUri = "/api/v1/account/login";
    private const string LogoutUri = "/api/v1/account/logout";
    private const string OverviewUri = "/api/v1/home/overview";
    private const string CreateNewAssetUri = "/api/v1/home/overview/createNewAsset";
    private const string EditAssetUri = "/api/v1/home/overview/edit/";
    private const string ReserveAssetUri = "/api/v1/home/overview/reserve/";
    private const string ReturnAssetUri = "/api/v1/home/overview/return/";
    private const string RemoveReservationUri = "/api/v1/home/overview/remove-reservation/";
    private const string ChangeReservationTimeUri = "/api/v1/home/overview/changeReservationTime/";
    private const string RemoveAssetUri = "/api/v1/home/overview/remove/";

    private static readonly Guid Category1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Category2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid Location1 = Guid.Parse("00000000-0000-0000-0000-000000000400");
    private static readonly Guid Location2 = Guid.Parse("00000000-0000-0000-0000-000000000401");

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly AppDbContext _dbContext;
    private readonly Guid _userId;
    private readonly Guid _ownerId;

    public HappyFlowTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        _output = output;

        _dbContext = _factory.Services.GetRequiredService<AppDbContext>();

        // Login populates the auth cookies on the shared client; subsequent requests are authenticated via cookies.
        Login().Wait();

        _userId = _dbContext.Users
            .Where(u => u.Username == _factory.GetUsername)
            .Select(u => u.Id)
            .First();

        _ownerId = _dbContext.Owners.Where(o => o.OwnerName.Equals(_factory.GetUsername)).Select(o => o.Id).First();

        _output.WriteLine("Test instance created: " + Guid.NewGuid());
        var env = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        _output.WriteLine("ENV: " + env.EnvironmentName);
    }

    public void Dispose()
    {
        Logout().Wait();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetOverview_ReturnsOk()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.GetAsync(OverviewUri, ct);
        response.EnsureSuccessStatusCode();

        var overview = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase, ct);
        Assert.NotNull(overview);
        Assert.NotNull(overview.AvailableAssets);
        Assert.NotNull(overview.AssetsReservedByUser);
    }

    [Fact]
    public async Task CreateNewAsset()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.GetAsync(OverviewUri, ct);
        response.EnsureSuccessStatusCode();

        response = await _client.PostAsJsonAsync(
            CreateNewAssetUri,
            new AssetViewModelCreate
            {
                AssetName = "test asset",
                Comment = "testing",
                SerialNumber = "SN-TEST-001",
                Barcode = "BC-TEST-001",
                SelectedCategoryId = Category1,
                SelectedLocationId = Location2,
                SelectedOwnerId = _ownerId
            }, ct);

        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<AssetViewModel>(JsonHelper.CamelCase, ct);

        Assert.NotNull(responseData);
        Assert.Equal("test asset", responseData.AssetName);
        Assert.Equal("SN-TEST-001", responseData.SerialNumber);
        Assert.Equal("BC-TEST-001", responseData.Barcode);
        Assert.Equal("Category 1", responseData.CategoryName);
        Assert.Equal(_factory.GetUsername, responseData.OwnerName);
        Assert.Equal("Room 2", responseData.RoomName);
        Assert.Equal("Cupboard 2", responseData.CupboardName);
        Assert.Equal(2, responseData.ShelfNum);
        Assert.Equal(2, responseData.Column);
        Assert.InRange(responseData.AddedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );

        var assetInDb = await LoadAssetWithRelations(responseData.Id);
        Assert.NotNull(assetInDb);

        Assert.NotNull(assetInDb.CategoryAssetsCollection);
        Assert.NotEmpty(assetInDb.CategoryAssetsCollection);
        Assert.NotNull(assetInDb.CategoryAssetsCollection.First().Category);

        Assert.NotNull(assetInDb.LocationsAssetsCollection);
        Assert.NotEmpty(assetInDb.LocationsAssetsCollection);
        Assert.NotNull(assetInDb.LocationsAssetsCollection.First().Location);

        Assert.NotNull(assetInDb.OwnerAssets);
        Assert.NotEmpty(assetInDb.OwnerAssets);
        Assert.NotNull(assetInDb.OwnerAssets.First().Owner);

        Assert.Equal("test asset", assetInDb.AssetName);
        Assert.Equal("SN-TEST-001", assetInDb.SerialNumber);
        Assert.Equal("BC-TEST-001", assetInDb.Barcode);
        Assert.Equal("Category 1", assetInDb.CategoryAssetsCollection.First().Category!.CategoryName);
        Assert.Equal(_factory.GetUsername, assetInDb.OwnerAssets.First().Owner!.OwnerName);
        Assert.Equal("Room 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First()
            .Cupboard!.CupboardsInRooms!.First().Room!.RoomName);
        Assert.Equal("Cupboard 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First().Cupboard!.CodeName);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First().Location!.ShelfNum);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First().Location!.Column);

        Assert.InRange(assetInDb.CreatedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );

        response = await _client.GetAsync(OverviewUri, ct);
        response.EnsureSuccessStatusCode();

        var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase, ct);
        Assert.NotNull(overviewData);
        Assert.Contains(overviewData.AvailableAssets, a => a.Id == responseData.Id);
    }

    [Fact]
    public async Task EditAsset()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var assetInDb = await _dbContext.Assets
            .Include(a => a.CategoryAssetsCollection)
            .Include(a => a.LocationsAssetsCollection)
            .Include(a => a.OwnerAssets)
            .FirstOrDefaultAsync(a => a.Id.Equals(asset.Id), ct);

        Assert.NotNull(assetInDb);

        var response = await _client.PutAsJsonAsync(EditAssetUri + asset.Id,
            new AssetViewModelUpdate
            {
                AssetId = asset.Id,
                AssetName = "Edited asset name",
                Comment = "Edited comment",
                SerialNumber = "SN-EDIT-001",
                Barcode = "BC-EDIT-001",

                CategoryAssetsId = assetInDb.CategoryAssetsCollection!.First().Id,
                SelectedCategoryId = Category1,

                OwnerAssetsId = assetInDb.OwnerAssets!.First().Id,
                SelectedOwnerId = _ownerId,

                LocationAssetsId = assetInDb.LocationsAssetsCollection!.First().Id,
                SelectedLocationId = Location2,
            }, ct);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        assetInDb = await LoadAssetWithRelations(asset.Id);
        Assert.NotNull(assetInDb);
        Assert.Equal("Edited asset name", assetInDb.AssetName);
        Assert.Equal("Edited comment", assetInDb.Comment);
        Assert.Equal("SN-EDIT-001", assetInDb.SerialNumber);
        Assert.Equal("BC-EDIT-001", assetInDb.Barcode);
        Assert.Equal("Category 1", assetInDb.CategoryAssetsCollection!.First().Category!.CategoryName);
        Assert.Equal(_factory.GetUsername, assetInDb.OwnerAssets!.First().Owner!.OwnerName);
        Assert.Equal("Room 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First().Cupboard!.CupboardsInRooms!.First().Room!.RoomName);
        Assert.Equal("Cupboard 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First().Cupboard!.CodeName);
    }

    [Fact]
    public async Task ReserveAsset()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddHours(2);

        var response = await _client.PostAsJsonAsync(ReserveAssetUri + asset.Id,
            new AssetReservationCreate
            {
                AssetId = asset.Id,
                UserId = _userId,
                ReservationFrom = from,
                ReservationTo = to
            }, ct);

        response.EnsureSuccessStatusCode();
        var msg = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase, ct);
        Assert.NotNull(msg);
        Assert.Equal($"Asset: '{asset.Id}' was reserved.", msg.Messages.First());

        _dbContext.ChangeTracker.Clear();
        var reservation = await _dbContext.AssetReservations
            .SingleAsync(r => r.AssetId == asset.Id && r.UserId == _userId, ct);
        Assert.False(reservation.IsReturned);
        Assert.Equal(from, reservation.ReservationFrom, TimeSpan.FromSeconds(2));
        Assert.Equal(to, reservation.ReservationTo, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ReturnAsset()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        await ReserveViaApi(asset.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var response = await _client.PostAsync(ReturnAssetUri + asset.Id, null, ct);
        response.EnsureSuccessStatusCode();

        var msg = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase, ct);
        Assert.NotNull(msg);
        Assert.Equal($"Asset '{asset.AssetName}' marked as returned.", msg.Messages.First());

        _dbContext.ChangeTracker.Clear();
        var reservation = await _dbContext.AssetReservations
            .SingleAsync(r => r.AssetId == asset.Id && r.UserId == _userId, ct);
        Assert.True(reservation.IsReturned);
        Assert.InRange(reservation.ReservationTo,
            DateTime.UtcNow - TimeSpan.FromMinutes(1),
            DateTime.UtcNow + TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RemoveReservation()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        await ReserveViaApi(asset.Id, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var response = await _client.PostAsync(RemoveReservationUri + asset.Id, null, ct);
        response.EnsureSuccessStatusCode();

        var msg = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase, ct);
        Assert.NotNull(msg);
        Assert.Equal($"Reservation for asset: '{asset.Id}' was removed.", msg.Messages.First());

        _dbContext.ChangeTracker.Clear();
        var stillExists = await _dbContext.AssetReservations
            .AnyAsync(r => r.AssetId == asset.Id && r.UserId == _userId && !r.IsReturned, ct);
        Assert.False(stillExists);
    }

    [Fact]
    public async Task ChangeReservationTime()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var initialFrom = DateTime.UtcNow.AddHours(1);
        var initialTo = initialFrom.AddHours(2);
        await ReserveViaApi(asset.Id, initialFrom, initialTo);

        _dbContext.ChangeTracker.Clear();
        var reservation = await _dbContext.AssetReservations
            .SingleAsync(r => r.AssetId == asset.Id && r.UserId == _userId, ct);

        var newFrom = DateTime.UtcNow.AddDays(1);
        var newTo = newFrom.AddHours(3);

        var response = await _client.PutAsJsonAsync(ChangeReservationTimeUri + reservation.Id,
            new AssetReservationUpdate
            {
                AssetReservationId = reservation.Id,
                AssetId = asset.Id,
                UserId = _userId,
                ReservationFrom = newFrom,
                ReservationTo = newTo,
                IsReturned = false
            }, ct);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _dbContext.ChangeTracker.Clear();
        var updated = await _dbContext.AssetReservations.SingleAsync(r => r.Id == reservation.Id, ct);
        Assert.Equal(newFrom, updated.ReservationFrom, TimeSpan.FromSeconds(2));
        Assert.Equal(newTo, updated.ReservationTo, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task SearchOverview()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var response = await _client.GetAsync(OverviewUri + "?searchTerm=" + Uri.EscapeDataString(asset.AssetName), ct);
        response.EnsureSuccessStatusCode();

        var overview = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase, ct);
        Assert.NotNull(overview);
        Assert.Contains(overview.AvailableAssets, a => a.Id == asset.Id);

        response = await _client.GetAsync(OverviewUri + "?searchTerm=__no_asset_matches_this_term__", ct);
        response.EnsureSuccessStatusCode();
        overview = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase, ct);
        Assert.NotNull(overview);
        Assert.Empty(overview.AvailableAssets);
    }

    [Fact]
    public async Task RemoveAsset()
    {
        var ct = TestContext.Current.CancellationToken;
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var response = await _client.PostAsJsonAsync(RemoveAssetUri + asset.Id,
            new RemovedAssetsCreate
            {
                AssetId = asset.Id,
                Comment = "test remove"
            }, ct);
        response.EnsureSuccessStatusCode();

        var msg = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase, ct);
        Assert.NotNull(msg);
        Assert.Equal($"Asset: '{asset.Id}' was removed.", msg.Messages.First());

        var removed = await _dbContext.RemovedAssetsCollection
            .Include(ra => ra.Asset)
            .FirstOrDefaultAsync(a => a.AssetId.Equals(asset.Id), ct);

        Assert.NotNull(removed);
        Assert.NotNull(removed.Asset);
        Assert.Equal("test remove", removed.Comment);
    }

    private async Task ReserveViaApi(Guid assetId, DateTime from, DateTime to)
    {
        var response = await _client.PostAsJsonAsync(ReserveAssetUri + assetId,
            new AssetReservationCreate
            {
                AssetId = assetId,
                UserId = _userId,
                ReservationFrom = from,
                ReservationTo = to
            });
        response.EnsureSuccessStatusCode();
    }

    private Task<Domain.Asset?> LoadAssetWithRelations(Guid assetId) =>
        _dbContext.Assets
            .Include(a => a.CategoryAssetsCollection)!
            .ThenInclude(x => x.Category)
            .Include(a => a.LocationsAssetsCollection)!
            .ThenInclude(la => la.Location)
            .ThenInclude(l => l!.LocationsInCupboards)!
            .ThenInclude(lc => lc.Cupboard)
            .ThenInclude(c => c!.CupboardsInRooms)!
            .ThenInclude(cr => cr.Room)
            .Include(a => a.OwnerAssets)!
            .ThenInclude(x => x.Owner)
            .FirstOrDefaultAsync(a => a.Id.Equals(assetId));

    // Add asset with category, location and owner straight to db.
    private AssetViewModel AddAsset()
    {
        var asset = new Domain.Asset
        {
            Id = Guid.NewGuid(),
            AssetName = "AssetName-" + Guid.NewGuid().ToString("N")[..8],
            Comment = "Comment"
        };
        var categoryAsset = new Domain.CategoryAssets
        {
            AssetId = asset.Id,
            CategoryId = Category2,
        };
        var locationAsset = new Domain.LocationAssets
        {
            AssetId = asset.Id,
            LocationId = Location1
        };
        var ownerAsset = new Domain.OwnerAssets
        {
            AssetId = asset.Id,
            OwnerId = _ownerId,
        };

        _dbContext.Assets.Add(asset);
        _dbContext.CategoryAssetsCollection.Add(categoryAsset);
        _dbContext.LocationAssetsCollection.Add(locationAsset);
        _dbContext.OwnerAssets.Add(ownerAsset);

        _dbContext.SaveChanges();

        return _dbContext.Assets
            .Include(a => a.OwnerAssets)!
            .ThenInclude(oa => oa.Owner)
            .Include(a => a.LocationsAssetsCollection)!
            .ThenInclude(la => la.Location)
            .ThenInclude(l => l!.LocationsInCupboards)!
            .ThenInclude(lc => lc.Cupboard)
            .ThenInclude(c => c!.CupboardsInRooms)!
            .ThenInclude(cr => cr.Room)
            .Include(a => a.CategoryAssetsCollection)!
            .ThenInclude(ca => ca.Category)
            .Include(a => a.AssetReservations)
            .Where(a => a.Id == asset.Id)
            .Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetName = a.AssetName,
                CategoryName = a.CategoryAssetsCollection!
                    .Select(ca => ca.Category!.CategoryName).First(),
                OwnerName = a.OwnerAssets!.Select(oa => oa.Owner!.OwnerName).First(),
                RoomName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .SelectMany(lc => lc.Cupboard!.CupboardsInRooms!)
                    .Select(cr => cr.Room!.RoomName).First(),
                CupboardName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .Select(lc => lc.Cupboard!.CodeName).First(),
                ShelfNum = a.LocationsAssetsCollection!.Select(la => la.Location!.ShelfNum).FirstOrDefault(),
                Column = a.LocationsAssetsCollection!.Select(la => la.Location!.Column).FirstOrDefault(),
                AddedAt = a.LocationsAssetsCollection!.Select(la => la.CreatedAt).FirstOrDefault()
            }).First();
    }

    private async Task<IdentityResponse> Login()
    {
        var response = await _client.PostAsJsonAsync(
            LoginUri,
            new LoginRequest
            {
                Username = _factory.GetUsername,
                Password = _factory.GetPassword
            }
        );

        response.EnsureSuccessStatusCode();
        var contentStr = await response.Content.ReadAsStringAsync();

        var identity = JsonSerializer.Deserialize<IdentityResponse>(contentStr, JsonHelper.CamelCase);

        identity.Should().NotBeNull();
        identity!.Username.Should().Be(_factory.GetUsername);

        return identity;
    }

    private async Task<LogoutResponse> Logout()
    {
        var response = await _client.PostAsync(LogoutUri, content: null);
        response.EnsureSuccessStatusCode();
        var contentStr = await response.Content.ReadAsStringAsync();

        var logoutData = JsonSerializer.Deserialize<LogoutResponse>(contentStr, JsonHelper.CamelCase);

        logoutData.Should().NotBeNull();
        logoutData!.DeletedTokens.Should().Be(1);

        return logoutData;
    }
}