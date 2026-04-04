using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApp;

public static class DataSeeder
{
    private const string RoleAdmins = "admins";
    private const string RoleMembers = "members";
    private const string RolePixels = "pixels";
    private const string RoleHelpdeskDbAdmins = "helpdesk_db_admins";

    public static async Task SeedRoles(this IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var roles = new[] { RoleAdmins, RoleMembers, RolePixels, RoleHelpdeskDbAdmins };

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
        await context.SaveChangesAsync();
    }

    // public static async Task SeedAdminUser(this IApplicationBuilder applicationBuilder)
    // {
    //     using IServiceScope serviceScope = applicationBuilder.ApplicationServices
    //         .GetRequiredService<IServiceScopeFactory>()
    //         .CreateScope();
    //     await using var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    //     await context.Database.MigrateAsync();
    //     using var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    //     using var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    //     IdentityResult res;
    //
    //     if ((await userManager.GetUsersInRoleAsync(RoleAdmins)).Any()) return;
    //
    //     var user = new AppUser
    //     {
    //         FirstName = "Mark",
    //         LastName = "Perman",
    //         Email = "maperm@taltech.ee",
    //         UserName = "admin",
    //         SecurityStamp = Guid.NewGuid().ToString()
    //     };
    //
    //     res = await userManager.CreateAsync(user, "AdminSuperSecretPasswordJeuzee");
    //     if (!res.Succeeded)
    //     {
    //         Console.WriteLine(res.ToString());
    //         throw new ApplicationException("User creation failed!");
    //     }
    //     res = userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.FirstName)).Result;
    //     if (!res.Succeeded)
    //     {
    //         throw new ApplicationException("Claim adding failed!");
    //     }
    //     res = userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.LastName)).Result;
    //     if (!res.Succeeded)
    //     {
    //         throw new ApplicationException("Claim adding failed!");
    //     }
    //
    //
    //     var adminRole = await context.Roles.FirstAsync(r => r.Name!.Equals(RoleAdmins));
    //     var lapRole = await context.Roles.FirstAsync(r => r.Name!.Equals(RoleMembers));
    //     user = await userManager.FindByEmailAsync(AdminEmail);
    //
    //     var adminUserRole = new AppUserRole
    //     {
    //         Id = Guid.NewGuid(),
    //         UserId = user!.Id,
    //         RoleId = adminRole.Id
    //     };
    //
    //     var lapikudUserRole = new AppUserRole
    //     {
    //         Id = Guid.NewGuid(),
    //         UserId = user!.Id,
    //         RoleId = lapRole.Id
    //     };
    //
    //     await context.UserRoles.AddAsync(adminUserRole);
    //     await context.UserRoles.AddAsync(lapikudUserRole);
    //     await context.SaveChangesAsync();
    // }

    public static async Task SeedSampleData(this IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await SeedCategories(context);
        var rooms = await SeedRooms(context);
        var cupboards = await SeedCupboards(context);
        await SeedCupboardsInRoom(context, cupboards, rooms);
        var locations = await SeedLocations(context);
        await SeedLocationsInCupboards(context, cupboards, locations);
        await SeedOwners(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCategories(AppDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category
                {
                    CategoryName = "Tools",
                    Comment = "Hammer, Screwdriver, Multimeter, Soldering Iron",
                },
                new Category
                {
                    CategoryName = "Power & Charging",
                    Comment = "Power Supply, Battery, Charger, UPS"
                },
                new Category
                {
                    CategoryName = "Cables & Adapters",
                    Comment = "HDMI, VGA, USB, Power Cables, Converters"
                },
                new Category
                {
                    CategoryName = "Cooling & Thermal Management",
                    Comment = "Fans, Thermal Paste, Heatsinks, Cooling Pads"
                },
                new Category
                {
                    CategoryName = "Bonding & Sealing Materials",
                    Comment = "Super Glue, Epoxy, Tapes"
                },
            };
            await context.Categories.AddRangeAsync(categories);
        }
    }

    private static async Task<Room[]> SeedRooms(AppDbContext context)
    {
        if (!await context.Rooms.AnyAsync())
        {
            var rooms = new[]
            {
                new Room
                {
                    RoomName = "Front room",
                    Comment = "Main room"
                },
                new Room
                {
                    RoomName = "Back room",
                    Comment = "Backrooms"
                }
            };
            await context.Rooms.AddRangeAsync(rooms);
            return rooms;
        }

        return await context.Rooms.ToArrayAsync();
    }

    private static async Task<Cupboard[]> SeedCupboards(AppDbContext ctx)
    {
        if (!await ctx.Cupboards.AnyAsync())
        {
            var cupboards = new[]
            {
                new Cupboard
                {
                    CodeName = "CB01"
                },
                new Cupboard
                {
                    CodeName = "CB02"
                },
                new Cupboard
                {
                    CodeName = "CB03"
                },
                new Cupboard
                {
                    CodeName = "CB04"
                },
                new Cupboard
                {
                    CodeName = "CB05"
                },
            };
            await ctx.Cupboards.AddRangeAsync(cupboards.OrderBy(c => c.CodeName, StringComparer.OrdinalIgnoreCase));
            return cupboards;
        }

        return await ctx.Cupboards.ToArrayAsync();
    }

    private static async Task SeedCupboardsInRoom(AppDbContext ctx, Cupboard[] cupboards, Room[] rooms)
    {
        if (!await ctx.CupboardsInRooms.AnyAsync())
        {
            var cupboardIds = cupboards
                .ToDictionary(c => c.CodeName, c => c.Id, StringComparer.OrdinalIgnoreCase);

            var roomIds = rooms
                .ToDictionary(r => r.RoomName, r => r.Id, StringComparer.OrdinalIgnoreCase);

            var cupboardsInRooms = new List<CupboardInRoom>
            {
                new() { CupboardId = cupboardIds["CB01"], RoomId = roomIds["Front Room"], Comment = "Big white one" },
                new() { CupboardId = cupboardIds["CB02"], RoomId = roomIds["Front Room"], Comment = "Big black one" },
                new() { CupboardId = cupboardIds["CB03"], RoomId = roomIds["Front Room"], Comment = "Big blue one" },
                new() { CupboardId = cupboardIds["CB04"], RoomId = roomIds["Front Room"], Comment = "Big gray one" },
                new() { CupboardId = cupboardIds["CB05"], RoomId = roomIds["Back Room"], Comment = "Big xd one" },
            };

            await ctx.CupboardsInRooms.AddRangeAsync(cupboardsInRooms);
        }
    }

    private static async Task<Location[]> SeedLocations(AppDbContext ctx)
    {
        if (!await ctx.Locations.AnyAsync())
        {
            var locations = new List<Location>();
            for (var x = 1; x < 6; x++) // cupboard number
            {
                for (var i = 1; i < 5; i++) // shelf number
                {
                    for (var j = 1; j < 5; j++) // column number
                    {
                        locations.Add(
                            new Location
                            {
                                LocationName = $"CB{x:D2}_{i}{j}",
                                ShelfNum = i,
                                Column = j,
                            });
                    }
                }
            }

            await ctx.Locations.AddRangeAsync(locations);
            return locations.ToArray();
        }

        return await ctx.Locations.ToArrayAsync();
    }

    private static async Task SeedLocationsInCupboards(AppDbContext ctx, Cupboard[] cupboards, Location[] locations)
    {
        if (!await ctx.LocationsInCupboards.AnyAsync())
        {
            var cupboardIds = cupboards
                .ToDictionary(c => c.CodeName, c => c.Id, StringComparer.OrdinalIgnoreCase);

            var locationIds = locations
                .ToDictionary(l => l.LocationName, l => l.Id);

            var locsInCupboards = new List<LocationInCupboard>();

            foreach (var (codeName, cupboardId) in cupboardIds)
            {
                var matchingLocations = locationIds
                    .Where(l => l.Key.Contains(codeName))
                    .Select(l => l.Value);

                foreach (var locationId in matchingLocations)
                {
                    var locationInCupboard = new LocationInCupboard
                    {
                        LocationId = locationId,
                        CupboardId = cupboardId,
                    };
                    locsInCupboards.Add(locationInCupboard);
                }
            }

            await ctx.LocationsInCupboards.AddRangeAsync(locsInCupboards);
        }
    }

    private static async Task SeedOwners(AppDbContext ctx)
    {
        if (!await ctx.Owners.AnyAsync())
        {
            var owners = new[]
            {
                new Owner
                {
                    OwnerName = "Lapikud",
                    Comment = "Organisation"
                },
            };
            await ctx.Owners.AddRangeAsync(owners.OrderBy(o => o.OwnerName, StringComparer.OrdinalIgnoreCase));
            ;
        }
    }
}