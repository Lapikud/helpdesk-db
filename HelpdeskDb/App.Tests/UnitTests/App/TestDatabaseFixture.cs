using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Base.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace App.Tests.UnitTests.App;

public class TestDatabaseFixture
{
    internal const string WebRootPath = "";
    internal const string Email = "test.user@gmail.com";
    internal const string Username = "TestUser";
    internal const string FirstName = "Firstame";
    internal const string LastName = "Lastname";
    internal const string Password = "Asdasd1!";
    internal const string LapikudRole = "lapikud";
    internal static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0001-000000000000");
    internal static readonly Guid RoleId = Guid.Parse("00000000-0000-0001-0000-000000000000");
    internal static readonly Guid AppUserRoleId = Guid.Parse("00000000-1000-0000-0000-000000000000");
    
    private static readonly object Lock = new();
    private static bool _databaseInitialized;
    
    private readonly IUserNameResolver _userNameResolver;
    private readonly ILogger<AppDbContext> _logger;
    
    public TestDatabaseFixture()
    {
        // Set up mocks
        var userNameResolverMock = new Mock<IUserNameResolver>();
        userNameResolverMock.Setup(x => x.CurrentUserName).Returns(Username);

        var loggerMock = new Mock<ILogger<AppDbContext>>();

        _userNameResolver = userNameResolverMock.Object;
        _logger = loggerMock.Object;
        lock (Lock)
        {
            if (_databaseInitialized) return;

            using (var context = CreateContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                SeedData(context);

                context.SaveChanges();
            }

            _databaseInitialized = true;
        }
    }
    
    private static void SeedData(AppDbContext context)
    {
        var user = new AppUser
        {
            Id = UserId,
            Username = Username,
        };
        
        context.Users.Add(user);

        var role = new AppRole()
        {
            Id = RoleId,
            Name = LapikudRole,
        };
        
        context.Roles.Add(role);

        var appUserRole = new AppUserRole()
        {
            Id = AppUserRoleId,
            User = user,
            Role = role
        };

        context.UserRoles.Add(appUserRole);
        
        context.Categories.AddRange(
            new Category
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CategoryName = "TestCategory1",
                Comment = "Category 1 test",
            },
            new Category
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CategoryName = "TestCategory2",
                Comment = "Category 2 test",
            }
        );

        var room1 = new Room
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000100"),
            RoomName = "TestRoom1",
            Comment = "Room 1 test"
        };

        var room2 = new Room
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
            RoomName = "TestRoom2",
            Comment = "Room 2 test"
        };

        context.Rooms.AddRange(room1, room2);

        var cupboard1 = new Cupboard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000200"),
            CodeName = "TestCupboard1"
        };

        var cupboard2 = new Cupboard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000201"),
            CodeName = "TestCupboard2"
        };

        context.Cupboards.AddRange(cupboard1, cupboard2);

        context.CupboardsInRooms.AddRange(
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
            LocationName = "TestLocation1",
            ShelfNum = 1,
            Column = 1,
        };

        var location2 = new Location()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000401"),
            LocationName = "TestLocation2",
            ShelfNum = 2,
            Column = 2,
        };

        context.Locations.AddRange(location1, location2);

        context.LocationsInCupboards.AddRange(
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

        context.Owners.AddRange(
            new Owner()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000600"),
                OwnerName = "TestOwner1",
                Comment = "Owner 1 test"
            },
            new Owner()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000601"),
                OwnerName = "TestOwner2",
                Comment = "Owner 2 test"
            }
        );
        
        context.SaveChanges();
    }
    
    public AppDbContext CreateContext()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();
        
        var connectionString = configuration.GetConnectionString("TestDbConnection");
        
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString,
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .Options, _userNameResolver, _logger);
    }
}