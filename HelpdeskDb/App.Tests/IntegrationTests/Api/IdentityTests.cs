using System.Net;
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
    private const string? LoginCustomJWTExpirationUri = "/api/v1/account/login?jwtExpiresInSeconds=5";
    private const string AdminEndpointUri = "/api/v1/roles";

    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;


    public IdentityTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        var env = _factory.Services.GetRequiredService<IWebHostEnvironment>();
        _output.WriteLine("ENV: " + env.EnvironmentName);
    }

    // Each test gets its own client so cookie state does not leak between tests.
    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private static void AssertAuthCookiesPresent(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        var cookieList = cookies!.ToList();
        Assert.Contains(cookieList, c => c.StartsWith($"{AccountController.JwtCookieName}="));
        Assert.Contains(cookieList, c => c.StartsWith($"{AccountController.RefreshCookieName}="));
        Assert.Contains(cookieList, c => c.Contains("httponly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        var loginData = new LoginRequest
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        var response = await client.PostAsJsonAsync(LoginUri, loginData, cancellationToken: ct);

        response.EnsureSuccessStatusCode();
        AssertAuthCookiesPresent(response);

        var identity = await response.Content.ReadFromJsonAsync<IdentityResponse>(ct);
        Assert.NotNull(identity);
        Assert.Equal(_factory.GetUsername, identity!.Username);
        Assert.NotEqual(Guid.Empty, identity.Id);
    }

    [Fact]
    public async Task Login_Check_Rights()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        var loginData = new LoginRequest
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        var response = await client.PostAsJsonAsync(LoginUri, loginData, cancellationToken: ct);
        response.EnsureSuccessStatusCode();
        AssertAuthCookiesPresent(response);

        // Cookies are auto-attached by HandleCookies=true; no Authorization header needed.
        var getResponse = await client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task No_Bearer_Header_Unauthorized()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        var getResponse = await client.GetAsync(CategoriesUri, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
    }

    [Fact]
    public async Task JWT_Custom_Expiration()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        var loginData = new LoginRequest
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        var response = await client.PostAsJsonAsync(LoginCustomJWTExpirationUri, loginData, cancellationToken: ct);
        response.EnsureSuccessStatusCode();
        AssertAuthCookiesPresent(response);

        var getResponse = await client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();

        // Wait for JWT to expire (cookie itself outlives the JWT — server enforces JWT exp)
        await Task.Delay(5500, ct);
        var getResponseAuthExpired = await client.GetAsync(CategoriesUri, ct);

        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);
    }

    [Fact]
    public async Task JWT_Refresh()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        var loginData = new LoginRequest
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        var response = await client.PostAsJsonAsync(LoginCustomJWTExpirationUri, loginData, cancellationToken: ct);
        response.EnsureSuccessStatusCode();
        AssertAuthCookiesPresent(response);

        var getResponse = await client.GetAsync(CategoriesUri, ct);
        getResponse.EnsureSuccessStatusCode();

        await Task.Delay(5500, ct);

        var getResponseAuthExpired = await client.GetAsync(CategoriesUri, ct);
        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);

        // Refresh — cookies are sent automatically; no body needed.
        var refreshResponse = await client.PostAsync(RenewRefreshTokenUri, content: null, ct);
        refreshResponse.EnsureSuccessStatusCode();
        AssertAuthCookiesPresent(refreshResponse);

        var refreshedIdentity = await refreshResponse.Content.ReadFromJsonAsync<IdentityResponse>(ct);
        Assert.NotNull(refreshedIdentity);

        var getResponse2 = await client.GetAsync(CategoriesUri, ct);
        getResponse2.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_With_Invalid_Password_Returns_NotFound()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = CreateClient();
        var fakeIpa = (FakeIpaAuthClient)_factory.Services.GetRequiredService<IIpaAuthClient>();
        fakeIpa.LoginShouldSucceed = false;
        try
        {
            var response = await client.PostAsJsonAsync(LoginUri, new LoginRequest
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
        using var client = CreateClient();
        var fakeIpa = (FakeIpaAuthClient)_factory.Services.GetRequiredService<IIpaAuthClient>();
        var originalGroups = fakeIpa.Groups;
        fakeIpa.Groups = new[] { "members" };
        try
        {
            var loginResp = await client.PostAsJsonAsync(LoginUri, new LoginRequest
            {
                Username = _factory.GetUsername,
                Password = _factory.GetPassword
            }, cancellationToken: ct);
            loginResp.EnsureSuccessStatusCode();

            var getResp = await client.GetAsync(AdminEndpointUri, ct);
            Assert.Equal(HttpStatusCode.Forbidden, getResp.StatusCode);
        }
        finally
        {
            fakeIpa.Groups = originalGroups;
        }
    }
}
