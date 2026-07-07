using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using App.DAL.EF;
using App.DTO.v1;
using App.DTO.v1.Identity;
using Asp.Versioning;
using Base.Helpers;
using FreeIPA.DotNet.Models;
using FreeIPA.DotNet.Models.Login;
using FreeIPA.DotNet.Models.RPC;
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
    private readonly IIpaAuthClient _ipaClient;

    private readonly Random _random = new Random();

    private const string UserPassProblem = "User/Password problem";
    private const int RandomDelayMin = 500;
    private const int RandomDelayMax = 5000;

    private const string SettingsJWTPrefix = "JWTSecurity";
    private const string SettingsJWTKey = SettingsJWTPrefix + ":Key";
    private const string SettingsJWTIssuer = SettingsJWTPrefix + ":Issuer";
    private const string SettingsJWTAudience = SettingsJWTPrefix + ":Audience";
    private const string SettingsJWTExpiresInSeconds = SettingsJWTPrefix + ":ExpiresInSeconds";

    private const string SettingsIpaServiceAccountUsername = "IpaServiceAccount:Username";
    private const string SettingsIpaServiceAccountPassword = "IpaServiceAccount:Password";

    public const string JwtCookieName = "hd_jwt";
    public const string RefreshCookieName = "hd_rt";
    private const string JwtCookiePath = "/api";
    // RFC 6265 says cookie Path is matched case-sensitively, so this must be lowercase to
    // match request URLs the frontend / tests actually use (e.g. /api/v1/account/login).
    private const string RefreshCookiePath = "/api/v1/account";


    /// <summary>
    /// Constructor
    /// </summary>
    public AccountController(IConfiguration configuration, ILogger<AccountController> logger, AppDbContext context, IIpaAuthClient ipaClient)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _ipaClient = ipaClient;
    }


    /// <summary>
    /// Log in a user. JWT and refresh token are returned as HttpOnly cookies; the response body
    /// contains only the identity needed by the client to render the UI.
    /// </summary>
    /// <param name="loginRequest">The login request</param>
    /// <param name="jwtExpiresInSeconds">The expiration time of the JWT in seconds</param>
    /// <returns>The user identity (id, username, roles)</returns>
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status404NotFound)]
    [HttpPost]
    public async Task<ActionResult<IdentityResponse>> Login(
        [FromBody] LoginRequest loginRequest,
        [FromQuery] int? jwtExpiresInSeconds
    )
    {
        try
        {
            IpaResultModel<IpaLoginResponseModel> loginResult =
                await _ipaClient.LoginWithPassword(new IpaLoginRequestModel()
                {
                    Username = loginRequest.Username,
                    Password = loginRequest.Password
                });
            if (loginResult.Success)
            {
                _logger.LogInformation("User: {Username} logged in.", loginRequest.Username);

                var user = _context.Users.FirstOrDefault(u => u.Username == loginRequest.Username);

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

                await SyncRolesFromIpaAsync(user);

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

                var (jwt, _) = await CreateJwt(user, jwtExpiresInSeconds);
                SetAuthCookies(jwt, refreshToken.RefreshToken, refreshToken.Expiration);

                var roleNames = await GetRoleNamesAsync(user.Id);
                return Ok(new IdentityResponse
                {
                    Id = user.Id,
                    Username = user.Username!,
                    Roles = roleNames
                });
            }
            else
            {
                ModelState.AddModelError(string.Empty, Base.Resources.Errors.IdentityErrors.InvalidLogin);
                await RandomDelayAsync();
                return NotFound(new Message(Base.Resources.Errors.IdentityErrors.InvalidLogin));
            }
        }
        catch (Exception e)
        {
            // Log server-side so an IPA outage isn't silently masked as "invalid login",
            // but still return the same generic message + delay to the caller.
            _logger.LogWarning(e, "Login failed for {Username}", loginRequest.Username);
            await RandomDelayAsync();
            return NotFound(new Message(Base.Resources.Errors.IdentityErrors.InvalidLogin));
        }
    }

    // Randomized delay on failed logins to blunt username-enumeration / timing side channels.
    private Task RandomDelayAsync() => Task.Delay(_random.Next(RandomDelayMin, RandomDelayMax));


    /// <summary>
    /// Logout the user. Reads the refresh token from the HttpOnly cookie, deletes it server-side,
    /// and clears both auth cookies on the response.
    /// </summary>
    /// <returns>The number of deleted refresh tokens</returns>
    [Produces("application/json")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<ActionResult<LogoutResponse>> Logout()
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
        }

        var appUser = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == User.GetUserId());

        if (appUser == null)
        {
            return NotFound(
                new Message(UserPassProblem)
            );
        }

        var refreshTokenFromCookie = Request.Cookies[RefreshCookieName];
        var deleteCount = 0;

        if (!string.IsNullOrEmpty(refreshTokenFromCookie))
        {
            // Delete only the token tied to this session's cookie (or its just-rotated
            // predecessor) so logging out on one device does not kill the user's other
            // sessions. The previous code discarded this query's result and removed every
            // token loaded via Include.
            var tokensToDelete = await _context.Entry(appUser)
                .Collection(u => u.RefreshTokens!)
                .Query()
                .Where(x =>
                    (x.RefreshToken == refreshTokenFromCookie) ||
                    (x.PreviousRefreshToken == refreshTokenFromCookie)
                )
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(tokensToDelete);

            deleteCount = await _context.SaveChangesAsync();
        }

        ClearAuthCookies();

        return Ok(new LogoutResponse
        {
            DeletedTokens = deleteCount
        });
    }

    /// <summary>
    /// Renew JWT using the refresh token. Both tokens are read from HttpOnly cookies and the
    /// rotated tokens are returned the same way; the response body contains the user identity.
    /// </summary>
    /// <param name="jwtExpiresInSeconds">Optional custom expiration for jwt</param>
    [Produces("application/json")]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Message), StatusCodes.Status401Unauthorized)]
    [HttpPost]
    public async Task<ActionResult<IdentityResponse>> RenewRefreshToken(
        [FromQuery] int? jwtExpiresInSeconds
    )
    {
        var jwtFromCookie = Request.Cookies[JwtCookieName];
        var refreshFromCookie = Request.Cookies[RefreshCookieName];

        if (string.IsNullOrEmpty(jwtFromCookie) || string.IsNullOrEmpty(refreshFromCookie))
        {
            return BadRequest(new Message("Missing auth cookies"));
        }

        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtFromCookie);
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
                jwtFromCookie,
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

        var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (appUser == null)
        {
            return NotFound($"User with name {username} not found");
        }

        var validTokens = await _context.Entry(appUser).Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == refreshFromCookie && x.Expiration > DateTime.UtcNow) ||
                (x.PreviousRefreshToken == refreshFromCookie &&
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

        var refreshToken = appUser.RefreshTokens.First();

        if (refreshToken.RefreshToken == refreshFromCookie)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpiration = DateTime.UtcNow.AddMinutes(1);

            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.Expiration = DateTime.UtcNow.AddDays(7);
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        try
        {
            await SyncRolesFromIpaAsync(appUser);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Role sync threw on RenewRefreshToken for {Username}; continuing with existing roles", appUser.Username);
        }

        var (newJwt, _) = await CreateJwt(appUser, jwtExpiresInSeconds);
        SetAuthCookies(newJwt, refreshToken.RefreshToken, refreshToken.Expiration);

        var roleNames = await GetRoleNamesAsync(appUser.Id);
        return Ok(new IdentityResponse
        {
            Id = appUser.Id,
            Username = appUser.Username!,
            Roles = roleNames
        });
    }

    /// <summary>
    /// Returns the current authenticated user's identity. Used by the SPA to rehydrate state on
    /// page load when the JWT lives in an HttpOnly cookie and is not readable from JavaScript.
    /// </summary>
    [Produces("application/json")]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<ActionResult<IdentityResponse>> Me()
    {
        var userId = User.GetUserId();
        var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (appUser == null)
        {
            return Unauthorized();
        }

        var roleNames = await GetRoleNamesAsync(appUser.Id);
        return Ok(new IdentityResponse
        {
            Id = appUser.Id,
            Username = appUser.Username!,
            Roles = roleNames
        });
    }

    // Reads the user's IPA group memberships via user_show RPC and reconciles them
    // with the local AppUserRole rows. Returns true only when DB rows were actually
    // added or removed; returns false on no-op AND on RPC failure (existing roles
    // are left untouched on failure so an IPA outage doesn't strip a user mid-session).
    private async Task<bool> SyncRolesFromIpaAsync(AppUser user)
    {
        var saUser = _configuration.GetValue<string>(SettingsIpaServiceAccountUsername);
        var saPass = _configuration.GetValue<string>(SettingsIpaServiceAccountPassword);
        if (string.IsNullOrEmpty(saUser) || string.IsNullOrEmpty(saPass))
        {
            _logger.LogWarning("IPA service account not configured; skipping role sync for {Username}", user.Username);
            return false;
        }

        var saLogin = await _ipaClient.LoginWithPassword(new IpaLoginRequestModel
        {
            Username = saUser,
            Password = saPass
        });
        if (!saLogin.Success)
        {
            _logger.LogWarning("IPA service account login failed; skipping role sync for {Username}: {Message}",
                user.Username, saLogin.Message);
            return false;
        }

        var rpcRequest = new IpaRpcRequestModel
        {
            Id = 0,
            Method = "user_show",
            Parameters = new object[]
            {
                Array.Empty<string>(), new
                {
                    uid = user.Username,
                    all = false,
                    raw = false,
                    rights = true
                }
            },
            Version = "2.251"
        };

        var rpcResult = await _ipaClient.SendRpcRequest(rpcRequest);

        if (!rpcResult.Success)
        {
            _logger.LogWarning("IPA user_show failed for {Username}: {Message}",
                user.Username, rpcResult.Message);
            return false;
        }

        var doc = JsonDocument.Parse(rpcResult.Data!.Result!.ToString()!);
        var ipaGroups = doc.RootElement
            .GetProperty("result")
            .GetProperty("memberof_group")
            .EnumerateArray()
            .Select(e => e.GetString()!)
            .Where(g => g is "admins" or "members" or "pixels" or "helpdesk_db_admins")
            .ToHashSet();

        var currentRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id,
                (ur, r) => new { Ur = ur, RoleName = r.Name! })
            .ToListAsync();
        var currentRoleNames = currentRoles.Select(x => x.RoleName).ToHashSet();

        if (currentRoleNames.SetEquals(ipaGroups))
        {
            return false;
        }

        foreach (var entry in currentRoles.Where(x => !ipaGroups.Contains(x.RoleName)))
        {
            _context.UserRoles.Remove(entry.Ur);
        }
        foreach (var group in ipaGroups.Where(g => !currentRoleNames.Contains(g)))
        {
            var role = await _context.Roles.FirstAsync(r => r.Name == group);
            await _context.UserRoles.AddAsync(new AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }


    private async Task<(string Jwt, DateTime ExpiresAt)> CreateJwt(AppUser user, int? jwtExpiresInSeconds)
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

        var expiresAt = GetExpirationDateTime(jwtExpiresInSeconds, SettingsJWTExpiresInSeconds);
        var jwt = IdentityExtensions.GenerateJwt(
            principal.Claims,
            _configuration.GetValue<string>(SettingsJWTKey)!,
            _configuration.GetValue<string>(SettingsJWTIssuer)!,
            _configuration.GetValue<string>(SettingsJWTAudience)!,
            expiresAt
        );
        return (jwt, expiresAt);
    }

    private DateTime GetExpirationDateTime(int? expiresInSeconds, string settingsKey)
    {
        var configValue = _configuration.GetValue<int>(settingsKey);
        if (expiresInSeconds == null || expiresInSeconds <= 0 || expiresInSeconds > configValue)
            expiresInSeconds = configValue;
        return DateTime.UtcNow.AddSeconds(expiresInSeconds.Value);
    }

    private async Task<List<string>> GetRoleNamesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
            .ToListAsync();
    }

    // The JWT cookie is given the same expiry as the refresh token (not the JWT's own exp claim)
    // so that an expired JWT can still be presented to RenewRefreshToken alongside the refresh
    // token. The server validates the JWT signature/claims independently of the cookie lifetime.
    // SameSite=Strict only works while the frontend and backend are same-site
    // (same registrable domain, e.g. both on localhost or both under example.com).
    // If the SPA is deployed on a different registrable domain than the API, the
    // browser will drop these cookies on cross-site requests — switch to
    // SameSiteMode.None and force Secure=true (HTTPS-only) when that day comes.
    private void SetAuthCookies(string jwt, string refreshToken, DateTime refreshExpiresAt)
    {
        var secure = Request.IsHttps;

        Response.Cookies.Append(JwtCookieName, jwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = JwtCookiePath,
            Expires = refreshExpiresAt
        });

        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath,
            Expires = refreshExpiresAt
        });
    }

    private void ClearAuthCookies()
    {
        var secure = Request.IsHttps;

        Response.Cookies.Delete(JwtCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = JwtCookiePath
        });

        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath
        });
    }
}