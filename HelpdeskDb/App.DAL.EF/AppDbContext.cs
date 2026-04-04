using App.Domain;
using App.Domain.Identity;
using Base.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<CategoryAssets> CategoryAssetsCollection { get; set; } = default!;
    public DbSet<Cupboard> Cupboards { get; set; } = default!;
    public DbSet<CupboardInRoom> CupboardsInRooms { get; set; } = default!;
    public DbSet<Location> Locations { get; set; } = default!;
    public DbSet<LocationInCupboard> LocationsInCupboards { get; set; } = default!;
    public DbSet<Owner> Owners { get; set; } = default!;
    public DbSet<OwnerAssets> OwnerAssets { get; set; } = default!;
    public DbSet<Asset> Assets { get; set; } = default!;
    public DbSet<Room> Rooms { get; set; } = default!;
    public DbSet<LocationAssets> LocationAssetsCollection { get; set; } = default!;

    public DbSet<RemovedAssets> RemovedAssetsCollection { get; set; } = default!;
    public DbSet<AssetReservation> AssetReservations { get; set; } = default!;
    public DbSet<AppUser> Users { get; set; } = default!;
    public DbSet<AppRole> Roles { get; set; } = default!;
    public DbSet<AppUserRole> UserRoles { get; set; } = default!;
    public DbSet<AppRefreshToken> RefreshTokens { get; set; } = default!;

    private readonly IUserNameResolver _userNameResolver;
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options, IUserNameResolver userNameResolver,
        ILogger<AppDbContext> logger
    )
        : base(options)
    {
        _userNameResolver = userNameResolver;
        _logger = logger;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // remove cascade delete
        foreach (var relationship in builder.Model
                     .GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }


        builder.Entity<AppUserRole>().HasKey(a => a.Id);
        builder.Entity<AppUserRole>().HasIndex(a => new { a.UserId, a.RoleId }).IsUnique();

        builder.Entity<AppUserRole>()
            .HasOne(a => a.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(a => a.UserId);

        builder.Entity<AppUserRole>()
            .HasOne(a => a.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(a => a.RoleId);

        // Asset Configuration
        builder.Entity<Asset>(b =>
        {
            b.HasKey(a => a.Id);
            // b.HasIndex(a => a.AssetName);
            // b.HasIndex(a => a.LastTakenBy);
            b.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(a => a.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Category Configuration
        builder.Entity<Category>(b =>
        {
            b.HasKey(c => c.Id);
            // b.HasIndex(c => c.CategoryName).IsUnique();
            b.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(c => c.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Cupboard Configuration
        builder.Entity<Cupboard>(b =>
        {
            b.HasKey(c => c.Id);
            // b.HasIndex(c => c.CodeName).IsUnique();
            b.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(o => o.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Room Configuration
        builder.Entity<Room>(b =>
        {
            b.HasKey(r => r.Id);
            // b.HasIndex(r => r.RoomName).IsUnique();
            b.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(o => o.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Owner Configuration
        builder.Entity<Owner>(b =>
        {
            b.HasKey(o => o.Id);
            // b.HasIndex(o => o.OwnerName).IsUnique();
            b.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(o => o.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Location Configuration
        builder.Entity<Location>(b =>
        {
            b.HasKey(l => l.Id);
            // b.HasIndex(o => o.OwnerName).IsUnique();
            b.Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(l => l.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Junction Table Configurations
        ConfigureJunctionTables(builder);
    }

    private void ConfigureJunctionTables(ModelBuilder builder)
    {
        // CategoryAssets
        builder.Entity<CategoryAssets>(b =>
        {
            b.HasOne(ca => ca.Category)
                .WithMany(c => c.CategoryAssetsCollection)
                .HasForeignKey(ca => ca.CategoryId);

            b.HasOne(ca => ca.Asset)
                .WithMany(a => a.CategoryAssetsCollection)
                .HasForeignKey(ca => ca.AssetId);

            b.HasIndex(ca => new { ca.CategoryId, ca.AssetId }).IsUnique();
            b.Property(ca => ca.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(ca => ca.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // LocationAssets
        builder.Entity<LocationAssets>(b =>
        {
            b.HasOne(la => la.Location)
                .WithMany(l => l.LocationsAssetsCollection)
                .HasForeignKey(la => la.LocationId);

            b.HasOne(la => la.Asset)
                .WithMany(a => a.LocationsAssetsCollection)
                .HasForeignKey(la => la.AssetId);

            b.HasIndex(la => new { la.LocationId, la.AssetId }).IsUnique();
            b.Property(la => la.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(la => la.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // LocationInCupboard
        builder.Entity<LocationInCupboard>(b =>
        {
            b.HasOne(lic => lic.Location)
                .WithMany(l => l.LocationsInCupboards)
                .HasForeignKey(lic => lic.LocationId);

            b.HasOne(lic => lic.Cupboard)
                .WithMany(c => c.LocationsInCupboards)
                .HasForeignKey(lic => lic.CupboardId);

            b.HasKey(lic => lic.Id);
            b.HasIndex(lic => new { lic.LocationId, lic.CupboardId }).IsUnique();
            b.Property(lic => lic.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(lic => lic.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // CupboardInRoom
        builder.Entity<CupboardInRoom>(b =>
        {
            b.HasOne(cir => cir.Cupboard)
                .WithMany(c => c.CupboardsInRooms)
                .HasForeignKey(cir => cir.CupboardId);

            b.HasOne(cir => cir.Room)
                .WithMany(r => r.CupboardsInRooms)
                .HasForeignKey(cir => cir.RoomId);

            b.HasKey(cir => cir.Id);
            b.HasIndex(cir => new { cir.CupboardId, cir.RoomId }).IsUnique();
            b.Property(cir => cir.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(cir => cir.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // OwnerAssets
        builder.Entity<OwnerAssets>(b =>
        {
            b.HasOne(oa => oa.Owner)
                .WithMany(o => o.OwnerAssets)
                .HasForeignKey(oa => oa.OwnerId);

            b.HasOne(oa => oa.Asset)
                .WithMany(a => a.OwnerAssets)
                .HasForeignKey(oa => oa.AssetId);

            b.HasKey(oa => oa.Id);
            b.HasIndex(oa => new { oa.OwnerId, oa.AssetId }).IsUnique();
            b.Property(oa => oa.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(oa => oa.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // RemovedAssets
        builder.Entity<RemovedAssets>(b =>
        {
            b.HasOne(ra => ra.Asset)
                .WithMany(a => a.RemovedAssetsCollection)
                .HasForeignKey(ra => ra.AssetId);

            b.HasKey(ra => ra.Id);
            b.HasIndex(ua => new { ua.AssetId }).IsUnique();
            b.Property(ra => ra.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(ra => ra.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // AssetReservation
        builder.Entity<AssetReservation>(b =>
        {
            b.HasOne(ar => ar.User)
                .WithMany(a => a.AssetReservations)
                .HasForeignKey(ar => ar.UserId);

            b.HasOne(ar => ar.Asset)
                .WithMany(a => a.AssetReservations)
                .HasForeignKey(ar => ar.AssetId);

            b.HasKey(ar => ar.Id);

            b.Property(ar => ar.ReservationFrom)
                .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            b.Property(ar => ar.ReservationTo)
                .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            b.Property(ar => ar.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            b.Property(ar => ar.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var addedEntries = ChangeTracker.Entries();
        foreach (var entry in addedEntries)
        {
            if (entry is { Entity: IDomainMeta })
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        (entry.Entity as IDomainMeta)!.CreatedAt = DateTime.UtcNow;
                        (entry.Entity as IDomainMeta)!.CreatedBy = _userNameResolver.CurrentUserName;
                        break;
                    case EntityState.Modified:
                        entry.Property("ChangedAt").IsModified = true;
                        entry.Property("ChangedBy").IsModified = true;
                        (entry.Entity as IDomainMeta)!.ChangedAt = DateTime.UtcNow;
                        (entry.Entity as IDomainMeta)!.ChangedBy = _userNameResolver.CurrentUserName;

                        // Prevent overwriting CreatedBy/CreatedAt on update
                        entry.Property("CreatedAt").IsModified = false;
                        entry.Property("CreatedBy").IsModified = false;
                        break;
                }
            }

            if (entry is { Entity: IDomainUserId, State: EntityState.Modified })
            {
                // do not allow userid modification
                entry.Property("UserId").IsModified = false;
                _logger.LogWarning("UserId modification attempt. Denied!");
            }
        }


        return base.SaveChangesAsync(cancellationToken);
    }
}