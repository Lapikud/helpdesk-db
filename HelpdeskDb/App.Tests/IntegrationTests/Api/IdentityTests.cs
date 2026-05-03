using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using App.DTO.v1.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApp.ApiControllers.Identity;

namespace App.Tests.IntegrationTests.Api;

[Collection("Sequential")]
public class IdentityTests: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private const string? LoginUri = "/api/v1/account/login";
    private const string? CategoriesUri = "/api/v1/categories";
    private const string? RenewRefreshTokenUri = "/api/v1/account/RenewRefreshToken";
    private const string BearerScheme = "Bearer";
    private const string? LoginCustomJWTExpirationUri = "/api/v1/account/login?jwtExpiresInSeconds=5";

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;


    public IdentityTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        _output = output;
        var env = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        _output.WriteLine("ENV: " + env.EnvironmentName);
    }

    [Fact]
    public async Task Login()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginUri, loginData, cancellationToken: ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);
    }

    [Fact]
    public async Task Login_Check_Rights()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginUri, loginData, cancellationToken: ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);


        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);

        var getResponse = await _client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task No_Bearer_Header_Unauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        var getResponse = await _client.GetAsync(CategoriesUri, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
    }

    [Fact]
    public async Task JWT_Custom_Expiration()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginCustomJWTExpirationUri, loginData, cancellationToken: ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);


        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);

        var getResponse = await _client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();


        // Wait for JWT to expire
        await Task.Delay(5500, ct);
        var getResponseAuthExpired = await _client.GetAsync(CategoriesUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);
    }


    [Fact]
    public async Task JWT_Refresh()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginCustomJWTExpirationUri, loginData, cancellationToken: ct);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);


        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);

        var getResponse = await _client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();


        // Wait for JWT to expire
        await Task.Delay(5500, ct);

        var getResponseAuthExpired = await _client.GetAsync(CategoriesUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);

        // Refresh JWT
        var refreshResponse = await _client.PostAsJsonAsync(RenewRefreshTokenUri, new RefreshTokenRequest()
        {
            JWT = responseData.JWT,
            RefreshToken = responseData.RefreshToken
        }, cancellationToken: ct);

        var refreshedResponseData = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>(ct);
        Assert.NotNull(refreshedResponseData);


        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, refreshedResponseData.JWT);

        var getResponse2 = await _client.GetAsync(CategoriesUri, ct);
        getResponse2.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_With_Invalid_Password_Returns_NotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        var fakeIpa = (FakeIpaAuthClient)_factory.Services.GetRequiredService<IIpaAuthClient>();
        fakeIpa.LoginShouldSucceed = false;
        try
        {
            var response = await _client.PostAsJsonAsync(LoginUri, new LoginRequest
            {
                Username = _factory.GetUsername,
                Password = "wrong-password"
            }, cancellationToken: ct);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            fakeIpa.LoginShouldSucceed = true;
        }
    }

    [Fact]
    public async Task Non_Admin_User_Cannot_Access_Admin_Endpoint_Returns_Forbidden()
    {
        var ct = TestContext.Current.CancellationToken;
        var fakeIpa = (FakeIpaAuthClient)_factory.Services.GetRequiredService<IIpaAuthClient>();
        var originalGroups = fakeIpa.Groups;
        fakeIpa.Groups = new[] { "members" };
        try
        {
            var loginResp = await _client.PostAsJsonAsync(LoginUri, new LoginRequest
            {
                Username = _factory.GetUsername,
                Password = _factory.GetPassword
            }, cancellationToken: ct);
            loginResp.EnsureSuccessStatusCode();
            var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(ct);

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(BearerScheme, login!.JWT);

            var getResp = await _client.GetAsync($"/api/v1/roles", ct);

            Assert.Equal(HttpStatusCode.Forbidden, getResp.StatusCode);
        }
        finally
        {
            fakeIpa.Groups = originalGroups;
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
