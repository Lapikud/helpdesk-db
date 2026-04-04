using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using App.DAL.EF;
using App.Domain.Identity;
using App.DTO.v1;
using App.DTO.v1.Identity;
using Asp.Versioning;
using Base.Helpers;
using FreeIPA.DotNet;
using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppRefreshToken = App.Domain.Identity.AppRefreshToken;
using AppUser = App.Domain.Identity.AppUser;
using AppUserRole = App.Domain.Identity.AppUserRole;
using Owner = App.Domain.Owner;

namespace WebApp.ApiControllers.Identity;

/// <summary>
/// API controller for managing accounts.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;
    private readonly AppDbContext _context;

    private readonly Random _random = new Random();

    private const string UserPassProblem = "User/Password problem";
    private const int RandomDelayMin = 500;
    private const int RandomDelayMax = 5000;

    private const string SettingsJWTPrefix = "JWTSecurity";
    private const string SettingsJWTKey = SettingsJWTPrefix + ":Key";
    private const string SettingsJWTIssuer = SettingsJWTPrefix + ":Issuer";
    private const string SettingsJWTAudience = SettingsJWTPrefix + ":Audience";
    private const string SettingsJWTExpiresInSeconds = SettingsJWTPrefix + ":ExpiresInSeconds";


    /// <summary>
    /// Constructor
    /// </summary>
    public AccountController(IConfiguration configuration, ILogger<AccountController> logger, AppDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }


    /// <summary>
    /// Log in a user
    /// </summary>
    /// <param name="loginRequest">The login request</param>
    /// <param name="jwtExpiresInSeconds">The expiration time of the JWT in seconds</param>
    /// <returns>The JWT, refresh token, username and email</returns>
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RefreshTokenRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status404NotFound)]
    [HttpPost]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest loginRequest,
        [FromQuery] int? jwtExpiresInSeconds
    )
    {
        try
        {
            var ipaClient = new IpaClient("https://ipa.lapikud.ee");
            IpaResultModel<IpaLoginResponseModel> loginResult =
                await ipaClient.LoginWithPassword(new IpaLoginRequestModel()
                {
                    Username = loginRequest.Username,
                    Password = loginRequest.Password
                });
            if (loginResult.Success)
            {
                _logger.LogInformation($"User: {loginRequest.Username} logged in.");

                var user = _context.Users.FirstOrDefault(u => u.Username == loginRequest.Username);
                var groups = new List<string>();

                var rpcRequest = new IpaRpcRequestModel
                {
                    Id = 0,
                    Method = "user_show",
                    Parameters = new object[]
                    {
                        Array.Empty<string>(), new
                        {
                            uid = loginRequest.Username,
                            all = false,
                            raw = false,
                            rights = true
                        }
                    },
                    Version = "2.251"
                };

                var rpcResult = await ipaClient.SendRpcRequest(rpcRequest);

                if (rpcResult.Success)
                {
                    // RPC request successful, use the results
                    var response = rpcResult.Data!.Result!;
                    var doc = JsonDocument.Parse(response.ToString()!);
                    var allGroups = doc.RootElement
                        .GetProperty("result")
                        .GetProperty("memberof_group")
                        .EnumerateArray()
                        .Select(e => e.GetString())
                        .ToList();

                    foreach (var group in allGroups)
                    {
                        if (group is "admins" or "members" or "pixels" or "helpdesk_db_admins")
                        {
                            groups.Add(group);
                        }

                        Console.WriteLine($"RPC response groups: {group}");
                    }
                }
                else
                {
                    // RPC request failed, check the error message
                    var error = rpcResult.Data!.Error;
                    var errorMessage = rpcResult.Message;
                    Console.WriteLine($"RPC request failed: {error}");
                    Console.WriteLine($"RPC request failed message: {errorMessage}");
                }
                
                if (user == null)
                {
                    user = new AppUser
                    {
                        Id = Guid.NewGuid(),
                        Username = loginRequest.Username,
                    };

                    await _context.Users.AddAsync(user);
                }
                
                if (!_context.Owners.Any(o => o.OwnerName.Equals(user.Username)))
                {
                    await _context.Owners.AddAsync(new Owner()
                        { OwnerName = user.Username, Comment = "member" });
                }
                
                var existingUserRoles = _context.UserRoles.Where(ur => ur.UserId.Equals(user.Id)).ToList();
                foreach (var userRole in existingUserRoles)
                {
                    var role = _context.Roles.First(r => r.Id == userRole.RoleId);

                    if (!groups.Contains(role.Name))
                    {
                        _context.UserRoles.Remove(userRole);
                    }
                }
                foreach (var group in groups)
                {
                    var role = _context.Roles.First(r => r.Name!.Equals(group));
                    if (_context.UserRoles.FirstOrDefault(ur =>
                            ur.UserId.Equals(user.Id) && ur.RoleId.Equals(role.Id)) == null)
                    {
                        var userRole = new AppUserRole
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            RoleId = role.Id
                        };
                        await _context.UserRoles.AddAsync(userRole);
                    }
                }

                // clean up expired refresh tokens
                // EF Core InMemory provider does not support ExecuteDeleteAsync, so skip during integration tests
                if (!_context.Database.ProviderName!.Contains("InMemory"))
                {
                    var deletedRows = await _context
                        .RefreshTokens
                        .Where(t => t.UserId == user.Id && t.Expiration < DateTime.UtcNow)
                        .ExecuteDeleteAsync();
                    _logger.LogInformation("Deleted {DeletedRows} refresh tokens", deletedRows);
                }

                var refreshToken = new AppRefreshToken()
                {
                    UserId = user.Id,
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                var responseData = new LoginResponse()
                {
                    JWT = await CreateJwt(user, jwtExpiresInSeconds),
                    RefreshToken = refreshToken.RefreshToken,
                    Username = user.Username!
                };

                return Ok(responseData);
            }
            else
            {
                ModelState.AddModelError(string.Empty, Base.Resources.Errors.IdentityErrors.InvalidLogin);
                return NotFound(new Message(Base.Resources.Errors.IdentityErrors.InvalidLogin));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return NotFound(new Message(Base.Resources.Errors.IdentityErrors.InvalidLogin));
        }
    }


    /// <summary>
    /// Logout the user
    /// </summary>
    /// <param name="logoutRequest">The logout request containing the refresh token</param>
    /// <returns>The number of deleted refresh tokens</returns>
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<ActionResult<LogoutResponse>> Logout([FromBody] LogoutRequest logoutRequest)
    {
        // delete the refresh token - so user is kicked out after jwt expiration
        // We do not invalidate the jwt on serverside - that would require pipeline modification and checking against db on every request
        // so client can actually continue to use the jwt until it expires (keep the jwt expiration time short ~1 min)

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !Guid.TryParse(userIdStr, out Guid userId))
        {
            return NotFound(
                new Message(UserPassProblem)
            );
            // return BadRequest(
            //     new v1_0.RestApiErrorResponse
            //     {
            //         Status = HttpStatusCode.BadRequest,
            //         Error = "Invalid user id"
            //     }
            // );
        }

        var appUser = await _context.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == User.GetUserId());

        if (appUser == null)
        {
            return NotFound(
                new Message(UserPassProblem)
            );
            // return NotFound(
            //     new v1_0.RestApiErrorResponse
            //     {
            //         Status = HttpStatusCode.NotFound,
            //         Error = "User not found"
            //     }
            // );
        }


        await _context.Entry(appUser)
            .Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == logoutRequest.RefreshToken) ||
                (x.PreviousRefreshToken == logoutRequest.RefreshToken)
            )
            .ToListAsync();

        foreach (var appRefreshToken in appUser.RefreshTokens!)
        {
            _context.RefreshTokens.Remove(appRefreshToken);
        }

        var deleteCount = await _context.SaveChangesAsync();

        return Ok(new LogoutResponse
        {
            DeletedTokens = deleteCount
        });
    }

    /// <summary>
    /// Renew JWT using refresh token
    /// </summary>
    /// <param name="refreshTokenRequest">Data for renewal</param>
    /// <param name="jwtExpiresInSeconds">Optional custom expiration for jwt</param>
    /// <returns></returns>
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status401Unauthorized)]
    [HttpPost]
    public async Task<ActionResult<LoginResponse>> RenewRefreshToken(
        [FromBody] RefreshTokenRequest refreshTokenRequest,
        [FromQuery] int? jwtExpiresInSeconds
    )
    {
        JwtSecurityToken jwtToken;
        // get user info from jwt
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokenRequest.JWT);
            if (jwtToken == null)
            {
                return BadRequest(new Message("No token"));
            }
        }
        catch (Exception e)
        {
            return BadRequest(new Message($"Cant parse the token, {e.Message}"));
        }

        // validate jwt, ignore expiration date
        if (!IdentityExtensions.ValidateJwt(
                refreshTokenRequest.JWT,
                _configuration.GetValue<string>(SettingsJWTKey)!,
                _configuration.GetValue<string>(SettingsJWTIssuer)!,
                _configuration.GetValue<string>(SettingsJWTAudience)!
            ))
        {
            return BadRequest(new Message("JWT validation fail"));
        }

        var username = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        if (username == null)
        {
            return BadRequest(new Message("No name in jwt"));
        }

        // get user and tokens
        var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (appUser == null)
        {
            return NotFound($"User with name {username} not found");
        }

        // load and compare refresh tokens
        var validTokens = await _context.Entry(appUser).Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == refreshTokenRequest.RefreshToken && x.Expiration > DateTime.UtcNow) ||
                (x.PreviousRefreshToken == refreshTokenRequest.RefreshToken &&
                 x.PreviousExpiration > DateTime.UtcNow)
            )
            .ToListAsync();

        appUser.RefreshTokens = validTokens;

        if (appUser.RefreshTokens == null || appUser.RefreshTokens.Count == 0)
        {
            return Unauthorized(new Message("Refresh token expired or not found"));
        }

        if (appUser.RefreshTokens.Count != 1)
        {
            return Unauthorized(new Message("Invalid refresh token state"));
        }

        // make new refresh token, keep old one still valid for some time
        var refreshToken = appUser.RefreshTokens.First();

        if (refreshToken.RefreshToken == refreshTokenRequest.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpiration = DateTime.UtcNow.AddMinutes(1);

            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.Expiration = DateTime.UtcNow.AddDays(7);
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        var res = new LoginResponse()
        {
            JWT = await CreateJwt(appUser, jwtExpiresInSeconds),
            RefreshToken = refreshToken.RefreshToken,
            Username = appUser.Username!,
        };

        return Ok(res);
    }


    private async Task<string> CreateJwt(AppUser user, int? jwtExpiresInSeconds)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var userRoles = await _context.UserRoles.Where(ur => ur.UserId.Equals(user.Id)).ToListAsync();

        foreach (var userRole in userRoles)
        {
            var role = await _context.Roles.FirstAsync(r => r.Id.Equals(userRole.RoleId));
            claims.Add(new Claim(ClaimTypes.Role, role.Name!));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var jwt = IdentityExtensions.GenerateJwt(
            principal.Claims,
            _configuration.GetValue<string>(SettingsJWTKey)!,
            _configuration.GetValue<string>(SettingsJWTIssuer)!,
            _configuration.GetValue<string>(SettingsJWTAudience)!,
            GetExpirationDateTime(jwtExpiresInSeconds, SettingsJWTExpiresInSeconds)
        );
        return jwt;
    }

    private DateTime GetExpirationDateTime(int? expiresInSeconds, string settingsKey)
    {
        var configValue = _configuration.GetValue<int>(settingsKey);
        if (expiresInSeconds == null || expiresInSeconds <= 0 || expiresInSeconds > configValue)
            expiresInSeconds = configValue;
        return DateTime.UtcNow.AddSeconds(expiresInSeconds.Value);
    }
}