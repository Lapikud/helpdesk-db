using System.Net.Http.Headers;
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
using Xunit.Abstractions;

namespace App.Tests.IntegrationTests.Api;

[Collection("NonParallel")]
public class HappyFlowTest : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private const string? LoginUri = "/api/v1/account/login";
    private const string? LogoutUri = "/api/v1/account/logout";
    private const string? OverviewUri = "/api/v1/home/overview";
    private const string? CreateNewAssetUri = "/api/v1/home/overview/createnewasset";
    private const string? TakeAssetUri = "/api/v1/home/overview/take/";
    private const string? ReturnAssetUri = "/api/v1/home/overview/return/";
    private const string? EditAssetUri = "/api/v1/home/overview/edit/";
    private const string? RemoveAssetUri = "/api/v1/home/overview/remove/";
    private const string BearerScheme = "Bearer";
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly AppDbContext _dbContext;
    private readonly LoginResponse _loginData;
    // private readonly IServiceScope _scope;

    public HappyFlowTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _output = output;

        _dbContext = _factory.Services.GetRequiredService<AppDbContext>();

        _loginData = Login().Result;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerScheme, _loginData.JWT);
        
        _output.WriteLine("Test instance created: " + Guid.NewGuid());
        var env = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        _output.WriteLine("ENV: " + env.EnvironmentName);
    }

    public void Dispose()
    {
        Logout(_loginData.RefreshToken).Wait();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task CreateNewAsset()
    {
        var response = await _client.GetAsync(OverviewUri);

        response.EnsureSuccessStatusCode();

        response = await _client.PostAsJsonAsync(
            CreateNewAssetUri,
            new AssetViewModelCreate
            {
                AssetName = "test asset",
                Comment = "testing",
                SelectedCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                SelectedLocationId = Guid.Parse("00000000-0000-0000-0000-000000000401"),
                SelectedOwnerId = Guid.Parse("00000000-0000-0000-0000-000000000600")
            });

        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<AssetViewModel>(JsonHelper.CamelCase);

        Assert.NotNull(responseData);
        Assert.Equal("test asset", responseData.AssetName);
        Assert.Equal("Category 1", responseData.CategoryName);
        Assert.Equal("Owner 1", responseData.OwnerName);
        Assert.Equal("Room 2", responseData.RoomName);
        Assert.Equal("Cupboard 2", responseData.CupboardName);
        Assert.Equal(2, responseData.ShelfNum);
        Assert.Equal(2, responseData.Column);
        Assert.InRange(responseData.AddedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );

        // using var scope = _factory.Services.CreateScope();
        // var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var assetInDb = await _dbContext.Assets
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
            .FirstOrDefaultAsync(a => a.Id.Equals(responseData.Id));

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
        Assert.Equal("Category 1", assetInDb.CategoryAssetsCollection.First().Category!.CategoryName);
        Assert.Equal("Owner 1", assetInDb.OwnerAssets.First().Owner!.OwnerName);
        Assert.Equal("Room 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First()
            .Cupboard!.CupboardsInRooms!.First().Room!.RoomName);
        Assert.Equal("Cupboard 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First().Cupboard!.CodeName);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First()
            .Location!.ShelfNum);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First()
            .Location!.Column);

        Assert.InRange(assetInDb.CreatedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );
        Assert.InRange(assetInDb.CategoryAssetsCollection!.First().CreatedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );
        Assert.InRange(assetInDb.LocationsAssetsCollection!.First().CreatedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );
        Assert.InRange(assetInDb.OwnerAssets!.First().CreatedAt,
            DateTime.UtcNow - TimeSpan.FromSeconds(10),
            DateTime.UtcNow + TimeSpan.FromSeconds(10)
        );

        response = await _client.GetAsync(OverviewUri);

        response.EnsureSuccessStatusCode();

        var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);

        Assert.NotNull(overviewData);
        Assert.NotEmpty(overviewData.AvailableAssets);
    }

    [Fact]
    public async Task EditAsset()
    {
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var response = await _client.GetAsync(OverviewUri);
        response.EnsureSuccessStatusCode();

        var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);

        Assert.NotNull(overviewData);
        Assert.NotEmpty(overviewData.AvailableAssets);
        
        var assetInDb = await _dbContext.Assets
            .Include(a => a.CategoryAssetsCollection)
            .Include(a => a.LocationsAssetsCollection)
            .Include(a => a.OwnerAssets)
            .FirstOrDefaultAsync(a => a.Id.Equals(asset.Id));

        Assert.NotNull(assetInDb);
        
        Assert.NotNull(assetInDb.CategoryAssetsCollection);
        Assert.NotEmpty(assetInDb.CategoryAssetsCollection);
        
        Assert.NotNull(assetInDb.LocationsAssetsCollection);
        Assert.NotEmpty(assetInDb.LocationsAssetsCollection);
        
        Assert.NotNull(assetInDb.OwnerAssets);
        Assert.NotEmpty(assetInDb.OwnerAssets);

        response = await _client.PutAsJsonAsync(EditAssetUri + asset.Id.ToString(),
            new AssetViewModelUpdate()
            {
                AssetId = asset.Id,
                AssetName = "Edited asset name",
                Comment = "Edited comment",
                
                CategoryAssetsId = assetInDb.CategoryAssetsCollection!.First().Id,
                SelectedCategoryId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                
                OwnerAssetsId = assetInDb.CategoryAssetsCollection!.First().Id,
                SelectedOwnerId = Guid.Parse("00000000-0000-0000-0000-000000000600"),
                
                LocationAssetsId = assetInDb.LocationsAssetsCollection!.First().Id,
                SelectedLocationId = Guid.Parse("00000000-0000-0000-0000-000000000401"),
            });
        
        response.EnsureSuccessStatusCode();
        
        assetInDb = await _dbContext.Assets
            .Include(a => a.CategoryAssetsCollection!)
            .ThenInclude(x => x.Category!)
            .Include(a => a.LocationsAssetsCollection!)
            .ThenInclude(la => la.Location!)
            .ThenInclude(l => l.LocationsInCupboards!)
            .ThenInclude(lc => lc.Cupboard)
            .ThenInclude(c => c!.CupboardsInRooms!)
            .ThenInclude(cr => cr.Room!)
            .Include(a => a.OwnerAssets!)
            .ThenInclude(x => x.Owner)
            .FirstOrDefaultAsync(a => a.Id.Equals(asset.Id));

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
        
        Assert.Equal("Edited asset name", assetInDb.AssetName);
        Assert.Equal("Category 1", assetInDb.CategoryAssetsCollection.First().Category!.CategoryName);
        Assert.Equal("Owner 1", assetInDb.OwnerAssets.First().Owner!.OwnerName);
        Assert.Equal("Room 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First()
            .Cupboard!.CupboardsInRooms!.First().Room!.RoomName);
        Assert.Equal("Cupboard 2", assetInDb.LocationsAssetsCollection!.First()
            .Location!.LocationsInCupboards!.First().Cupboard!.CodeName);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First()
            .Location!.ShelfNum);
        Assert.Equal(2, assetInDb.LocationsAssetsCollection.First()
            .Location!.Column);
    }

    // [Fact]
    // public async Task TakeAsset()
    // {
    //     var asset = AddAsset();
    //     _dbContext.ChangeTracker.Clear();
    //
    //     var response = await _client.GetAsync(OverviewUri);
    //
    //     response.EnsureSuccessStatusCode();
    //
    //     var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);
    //
    //     Assert.NotNull(overviewData);
    //     Assert.Empty(overviewData.TakenAssets);
    //
    //     response = await _client.PostAsync(TakeAssetUri + asset.Id.ToString(), null);
    //     response.EnsureSuccessStatusCode();
    //
    //     var responseData = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase);
    //
    //     Assert.NotNull(responseData);
    //     Assert.Equal($"Asset: '{asset.Id}' was taken.", responseData.Messages.First());
    //
    //     response = await _client.GetAsync(OverviewUri);
    //     response.EnsureSuccessStatusCode();
    //
    //     overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);
    //
    //     Assert.NotNull(overviewData);
    //     Assert.NotEmpty(overviewData.TakenAssets);
    //
    //     var assetInDb = await _dbContext.Assets
    //         .Include(a => a.CategoryAssetsCollection)
    //         .Include(a => a.LocationsAssetsCollection)
    //         .Include(a => a.OwnerAssets)
    //         .Include(a => a.UserAssetsCollection!)
    //         .ThenInclude(ua => ua.User)
    //         .FirstOrDefaultAsync(a => a.Id.Equals(asset.Id));
    //
    //     Assert.NotNull(assetInDb);
    //     
    //     Assert.NotNull(assetInDb.CategoryAssetsCollection);
    //     Assert.NotEmpty(assetInDb.CategoryAssetsCollection);
    //     
    //     Assert.NotNull(assetInDb.LocationsAssetsCollection);
    //     Assert.NotEmpty(assetInDb.LocationsAssetsCollection);
    //     
    //     Assert.NotNull(assetInDb.OwnerAssets);
    //     Assert.NotEmpty(assetInDb.OwnerAssets);
    //     
    //     Assert.NotNull(assetInDb.UserAssetsCollection);
    //     Assert.NotEmpty(assetInDb.UserAssetsCollection);
    //     Assert.NotNull(assetInDb.UserAssetsCollection.First().User);
    //     
    //     Assert.Equal(assetInDb.Id, assetInDb.UserAssetsCollection.First().AssetId);
    //     Assert.Equal(_factory.GetUserId, assetInDb.UserAssetsCollection.First().User!.Id);
    //     
    // }

    [Fact]
    public async Task ReturnAsset()
    {
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var response = await _client.GetAsync(OverviewUri);
        response.EnsureSuccessStatusCode();

        var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);

        Assert.NotNull(overviewData);
        Assert.NotEmpty(overviewData.AvailableAssets);

        // take asset (lastTakenBy so that lastTakenBy changes)
        // maybe unnecessary since it is done through api, but I like it that way
        response = await _client.PostAsync(TakeAssetUri + asset.Id.ToString(), null);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase);

        Assert.NotNull(responseData);
        Assert.Equal($"Asset: '{asset.Id}' was taken.", responseData.Messages.First());

        // return asset
        response = await _client.PostAsync(ReturnAssetUri + asset.Id.ToString(), null);
        response.EnsureSuccessStatusCode();

        responseData = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase);

        Assert.NotNull(responseData);
        Assert.Equal($"Asset: '{asset.Id}' was returned.", responseData.Messages.First());


        var assetInDb = await _dbContext.Assets
            .Include(a => a.CategoryAssetsCollection)
            .Include(a => a.LocationsAssetsCollection)
            .Include(a => a.OwnerAssets)
            .FirstOrDefaultAsync(a => a.Id.Equals(asset.Id));

        Assert.NotNull(assetInDb);
        
        Assert.NotNull(assetInDb.CategoryAssetsCollection);
        Assert.NotEmpty(assetInDb.CategoryAssetsCollection);
        
        Assert.NotNull(assetInDb.LocationsAssetsCollection);
        Assert.NotEmpty(assetInDb.LocationsAssetsCollection);
        
        Assert.NotNull(assetInDb.OwnerAssets);
        Assert.NotEmpty(assetInDb.OwnerAssets);
        
        // Assert.Equal("testUserLapikud", assetInDb.LastTakenBy);
    }

    [Fact]
    public async Task RemoveAsset()
    {
        var asset = AddAsset();
        _dbContext.ChangeTracker.Clear();

        var response = await _client.GetAsync(OverviewUri);
        response.EnsureSuccessStatusCode();

        var overviewData = await response.Content.ReadFromJsonAsync<AssetsOverviewViewModel>(JsonHelper.CamelCase);

        Assert.NotNull(overviewData);
        Assert.NotEmpty(overviewData.AvailableAssets);

        response = await _client.PostAsJsonAsync(RemoveAssetUri + asset.Id.ToString(),
            new AssetViewModelRemove
            {
                AssetId = asset.Id,
                Comment = "test remove"
            });
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<Message>(JsonHelper.CamelCase);

        Assert.NotNull(responseData);
        Assert.Equal($"Asset: '{asset.Id}' was removed.", responseData.Messages.First());

        var removedAssetInDb = await _dbContext.RemovedAssetsCollection
            .Include(ra => ra.Asset)
            .FirstOrDefaultAsync(a => a.AssetId.Equals(asset.Id));

        Assert.NotNull(removedAssetInDb);
        Assert.NotNull(removedAssetInDb.Asset);
        Assert.Equal("test remove", removedAssetInDb.Comment);
    }

    // Add asset with category, location and owner straight to db.
    private AssetViewModel AddAsset()
    {
        var asset = new Domain.Asset()
        {
            Id = Guid.NewGuid(),
            AssetName = "AssetName",
            Comment = "Comment"
        };
        var categoryAsset = new Domain.CategoryAssets()
        {
            AssetId = asset.Id,
            CategoryId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        };
        var locationAsset = new Domain.LocationAssets()
        {
            AssetId = asset.Id,
            LocationId = Guid.Parse("00000000-0000-0000-0000-000000000400")
        };
        var ownerAsset = new App.Domain.OwnerAssets
        {
            AssetId = asset.Id,
            OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000600"),
        };

        _dbContext.Assets.Add(asset);
        _dbContext.CategoryAssetsCollection.Add(categoryAsset);
        _dbContext.LocationAssetsCollection.Add(locationAsset);
        _dbContext.OwnerAssets.Add(ownerAsset);

        _dbContext.SaveChanges();

        var assetViewModel = _dbContext.Assets.Include(a => a.OwnerAssets)! // Load OwnerAssets
            .ThenInclude(oa => oa.Owner) // Load Owner
            .Include(a => a.LocationsAssetsCollection)! // Load LocationAssets
            .ThenInclude(la => la.Location) // Load Location
            .ThenInclude(l => l!.LocationsInCupboards)! // Load LocationInCupboard
            .ThenInclude(lc => lc.Cupboard) // Load Cupboard
            .ThenInclude(c => c!.CupboardsInRooms)! // Load CupboardInRoom
            .ThenInclude(cr => cr.Room) // Load Room
            .Include(a => a.CategoryAssetsCollection)! // Load CategoryAssets
            .ThenInclude(ca => ca.Category) // Load Category
            .Include(a => a.AssetReservations) // Load AssetReservations
            .Where(a => a.Id == asset.Id)
            .Select(a => new AssetViewModel
            {
                Id = a.Id,
                AssetName = a.AssetName,
                CategoryName = a.CategoryAssetsCollection!
                    .Select(ca => ca.Category!.CategoryName)
                    .First(),
                OwnerName = a.OwnerAssets!.Select(oa => oa.Owner!.OwnerName).First(),
                RoomName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .SelectMany(lc => lc.Cupboard!.CupboardsInRooms!)
                    .Select(cr => cr.Room!.RoomName)
                    .First(),
                CupboardName = a.LocationsAssetsCollection!
                    .SelectMany(la => la.Location!.LocationsInCupboards!)
                    .Select(lc => lc.Cupboard!.CodeName)
                    .First(),
                ShelfNum = a.LocationsAssetsCollection!.Select(la => la.Location!.ShelfNum).FirstOrDefault(),
                Column = a.LocationsAssetsCollection!.Select(la => la.Location!.Column).FirstOrDefault(),
                // LastTakenBy = a.LastTakenBy,
                AddedAt = a.LocationsAssetsCollection!.Select(la => la.CreatedAt).FirstOrDefault()
            }).First();

        return assetViewModel;
    }

    private async Task<LoginResponse> Login()
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

        var loginData = JsonSerializer.Deserialize<LoginResponse>(contentStr, JsonHelper.CamelCase);

        loginData.Should().NotBeNull();
        loginData.JWT.Should().NotBeNullOrEmpty();

        return loginData;
    }

    private async Task<LogoutResponse> Logout(string refreshToken)
    {
        var response = await _client.PostAsJsonAsync(
            LogoutUri,
            new LogoutRequest
            {
                RefreshToken = refreshToken
            }
        );
        response.EnsureSuccessStatusCode();
        var contentStr = await response.Content.ReadAsStringAsync();

        var logoutData = JsonSerializer.Deserialize<LogoutResponse>(contentStr, JsonHelper.CamelCase);

        logoutData.Should().NotBeNull();
        logoutData.DeletedTokens.Should().Be(1);

        return logoutData;
    }
}