using System.Net;
using System.Net.Http.Json;
using App.DTO.v1.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace App.Tests.IntegrationTests.Api;

[Collection("Sequential")]
public class IdentityTests: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private const string? RegisterUri = "/api/v1/account/register";
    private const string? LoginUri = "/api/v1/account/login";
    private const string? CategoriesUri = "/api/v1/categories";
    private const string? RenewRefreshTokenUri = "/api/v1/account/RenewRefreshToken";
    private const string BearerScheme = "Bearer";
    private const string? LoginCustomJWTExpirationUri = "/api/v1/account/login?jwtExpiresInSeconds=1";
    
    
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
    public async Task Login_Existing_User()
    {
        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginUri, loginData);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);
    }

    [Fact]
    public async Task Login_Existing_User_Check_Rights()
    {
        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginUri, loginData);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);
        
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);
        
        var getResponse = await _client.GetAsync(CategoriesUri);
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task No_Bearer_Header_Unauthorized()
    {
        var getResponse = await _client.GetAsync(CategoriesUri);
        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
    }
    
    [Fact]
    public async Task JWT_Custom_Expiration()
    {
        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/account/login?jwtExpiresInSeconds=1", loginData);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);
        
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);
        
        var getResponse = await _client.GetAsync(CategoriesUri);
        getResponse.EnsureSuccessStatusCode();

        
        // Wait for JWT to expire
        await Task.Delay(1500);
        var getResponseAuthExpired = await _client.GetAsync(CategoriesUri);
        
        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);
    }


    [Fact]
    public async Task JWT_Refresh()
    {
        // Arrange
        var loginData = new LoginRequest()
        {
            Username = _factory.GetUsername,
            Password = _factory.GetPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(LoginCustomJWTExpirationUri, loginData);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(responseData);
        Assert.True(responseData.JWT.Length > 128);
        Assert.True(responseData.RefreshToken.Length == Guid.NewGuid().ToString().Length);
        
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, responseData.JWT);
        
        var getResponse = await _client.GetAsync(CategoriesUri);
        getResponse.EnsureSuccessStatusCode();

        
        // Wait for JWT to expire
        await Task.Delay(1500);

        var getResponseAuthExpired = await _client.GetAsync(CategoriesUri);
        
        Assert.Equal(HttpStatusCode.Unauthorized, getResponseAuthExpired.StatusCode);
        
        // Refresh JWT
        var refreshResponse = await _client.PostAsJsonAsync(RenewRefreshTokenUri, new RefreshTokenRequest()
        {
            JWT = responseData.JWT,
            RefreshToken = responseData.RefreshToken
        });
        
        var refreshedResponseData = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refreshedResponseData);
        
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerScheme, refreshedResponseData.JWT);
        
        var getResponse2 = await _client.GetAsync(CategoriesUri);
        getResponse2.EnsureSuccessStatusCode();
    }
    
    
}
