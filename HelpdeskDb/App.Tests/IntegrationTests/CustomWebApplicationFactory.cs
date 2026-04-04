using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace App.Tests.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private const string RoleLapikud = "lapikud";
    private const string RoleGuestUser = "guest";
    private const string Username = "testUserLapikud";
    private const string Password = "Test123!";
    private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000001000");

    public string GetUsername => Username;
    public string GetPassword => Password;
    public Guid GetUserId => UserId;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove the existing DbContextOptions
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            var dbName = $"TEST_{Guid.NewGuid()}";
            var connectionString = $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseNpgsql(
                        connectionString,
                        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    )
                    .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    // disable tracking, allow id based shared entity creation
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            );


            // create db and seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<AppDbContext>();

            var logger = scopedServices
                .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            db.Database.EnsureDeleted();
            db.Database.Migrate();

            try
            {
                Task.Run(async () => { await SeedData(db); }).GetAwaiter().GetResult();
                logger.LogWarning("⚠️ Creating fresh test database at {Time}", DateTime.Now);

                // Optional: add a unique GUID or test name if you want to trace multiple runs
                logger.LogWarning("🔁 DB Reset ID: {Id}", Guid.NewGuid());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                                    "database with test messages. Error: {Message}", ex.Message);
            }
        });
    }

    private static async Task SeedData(AppDbContext context)
    {
        var roles = new[] { RoleLapikud, RoleGuestUser };
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

        var user = new AppUser
        {
            Id = UserId,
            Username = Username,
        };

        await context.Users.AddAsync(user);


        var lapRole = context.Roles.First(r => r.Name!.Equals(RoleLapikud));
        

        var lapikudUserRole = new AppUserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = lapRole.Id
        };

        await context.UserRoles.AddAsync(lapikudUserRole);

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

        await context.Owners.AddRangeAsync(
            new Owner()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000600"),
                OwnerName = "Owner 1",
                Comment = "Owner 1 test"
            }
        );

        await context.SaveChangesAsync();
    }
}