using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using WebApp.ApiControllers.Identity;

namespace App.Tests.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private const string RoleMember = "members";
    private const string RoleAdmins = "admins";
    private const string RolePixels = "pixels";
    private const string RoleHelpdeskDbAdmins = "helpdesk_db_admins";

    private const string Username = "testuser";

    public string GetUsername => Username;
    public string GetPassword => "not-a-real-password";

    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs reads configuration inline while the WebApplicationBuilder is being
        // set up (fail-fast JWT key validation, token validation parameters). With minimal
        // hosting, ConfigureAppConfiguration sources are merged in too late for those reads,
        // so without a .env on the machine the 64-byte key check would fail. UseSetting
        // lands in the builder's initial configuration and is visible to the inline reads.
        builder.UseSetting("ConnectionStrings:DefaultConnection", "DataSource=:memory:");
        builder.UseSetting("JWTSecurity:Key", "test-only-signing-key-please-do-not-use-in-production-1234567890");
        builder.UseSetting("JWTSecurity:Issuer", "LapikudHelpdesk");
        builder.UseSetting("JWTSecurity:Audience", "LapikudHelpdesk");
        builder.UseSetting("JWTSecurity:ExpiresInSeconds", "120");
        builder.UseSetting("IpaServiceAccount:Username", "test-service-account");
        builder.UseSetting("IpaServiceAccount:Password", "test-service-account-password");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json")
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                    ["JWTSecurity:Key"] = "test-only-signing-key-please-do-not-use-in-production-1234567890",
                    ["JWTSecurity:Issuer"] = "LapikudHelpdesk",
                    ["JWTSecurity:Audience"] = "LapikudHelpdesk",
                    ["JWTSecurity:ExpiresInSeconds"] = "120",
                    ["IpaServiceAccount:Username"] = "test-service-account",
                    ["IpaServiceAccount:Password"] = "test-service-account-password",
                })
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") ?? false) ||
                (d.ServiceType.FullName?.Contains("Npgsql") ?? false)
            ).ToList();
            foreach (var d in toRemove) services.Remove(d);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseSqlite(_connection)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            );

            services.RemoveAll<IIpaAuthClient>();
            services.AddSingleton<IIpaAuthClient, FakeIpaAuthClient>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<AppDbContext>();

            var logger = scopedServices
                .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            db.Database.EnsureCreated();

            try
            {
                Task.Run(async () => { await SeedData(db); }).GetAwaiter().GetResult();
                logger.LogWarning("SQLite in-memory test database ready at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                                    "database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }

    private static async Task SeedData(AppDbContext context)
    {
        var roles = new[] { RoleMember, RoleAdmins, RolePixels, RoleHelpdeskDbAdmins };
        var rolesToAdd = new List<AppRole>();
        foreach (var roleName in roles)
        {
            var roleInDb = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (roleInDb == null)
            {
                rolesToAdd.Add(new AppRole
                {
                    Name = roleName
                });
            }
        }
        await context.Roles.AddRangeAsync(rolesToAdd);

        await context.Categories.AddRangeAsync(
            new Category
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CategoryName = "Category 1",
                Comment = "Category 1 test",
            },
            new Category
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CategoryName = "Category 2",
                Comment = "Category 2 test",
            }
        );

        var room1 = new Room
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000100"),
            RoomName = "Room 1",
            Comment = "Room 1 test"
        };

        var room2 = new Room
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
            RoomName = "Room 2",
            Comment = "Room 2 test"
        };

        await context.Rooms.AddRangeAsync(room1, room2);

        var cupboard1 = new Cupboard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000200"),
            CodeName = "Cupboard 1"
        };

        var cupboard2 = new Cupboard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000201"),
            CodeName = "Cupboard 2"
        };

        await context.Cupboards.AddRangeAsync(cupboard1, cupboard2);

        await context.CupboardsInRooms.AddRangeAsync(
            new CupboardInRoom()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000300"),
                CupboardId = cupboard1.Id,
                RoomId = room1.Id,
                Comment = "Cupboard in room 1"
            },
            new CupboardInRoom()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000301"),
                CupboardId = cupboard2.Id,
                RoomId = room2.Id,
                Comment = "Cupboard in room 2"
            }
        );

        var location1 = new Location()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000400"),
            LocationName = "Location 1",
            ShelfNum = 1,
            Column = 1,
        };

        var location2 = new Location()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000401"),
            LocationName = "Location 2",
            ShelfNum = 2,
            Column = 2,
        };

        await context.Locations.AddRangeAsync(location1, location2);

        await context.LocationsInCupboards.AddRangeAsync(
            new LocationInCupboard()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000500"),
                LocationId = location1.Id,
                CupboardId = cupboard1.Id,
            },
            new LocationInCupboard()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000501"),
                LocationId = location2.Id,
                CupboardId = cupboard2.Id,
            }
        );

        await context.SaveChangesAsync();
    }
}
