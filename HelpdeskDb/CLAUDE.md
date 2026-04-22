# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from `HelpdeskDb/` (the solution root):

```bash
# Build
dotnet build HelpdeskDb.sln

# Run the web app
dotnet run --project WebApp/WebApp.csproj

# Run all tests
dotnet test HelpdeskDb.sln

# Run a single test class
dotnet test App.Tests/App.Tests.csproj --filter "FullyQualifiedName~HappyFlowTest"

# Run a single test method
dotnet test App.Tests/App.Tests.csproj --filter "FullyQualifiedName~HappyFlowTest.CreateNewAsset"

# Add EF migration
dotnet ef migrations add <MigrationName> --project App.DAL.EF --startup-project WebApp

# Apply migrations
dotnet ef database update --project App.DAL.EF --startup-project WebApp
```

## Architecture

This is an ASP.NET Core 10 asset-management helpdesk app (tracking physical IT assets stored in cupboards/rooms, with owners, categories, and reservations). The solution follows a strict layered architecture with manual mappers at each boundary — no AutoMapper.

### Layer dependency order (outer → inner)

```
WebApp
  └── App.BLL / App.BLL.Contracts
        └── App.DAL.EF / App.DAL.Contracts
              └── App.Domain
                    └── Base.* (reusable base classes)
App.DTO (used by WebApp for API contracts, versioned under v1/)
App.BLL.DTO (used by BLL services internally)
App.DAL.DTO (used by DAL repositories)
```

### Key projects

- **App.Domain** — EF entity classes, all inherit `BaseEntity` (which has `Id`, `CreatedAt`, `CreatedBy`, `ChangedAt`, `ChangedBy`)
- **App.DAL.EF** — `AppDbContext`, `AppUOW` (Unit of Work), repositories. Cascade delete is globally disabled (uses `DeleteBehavior.Restrict`). `SaveChangesAsync` auto-stamps audit fields and blocks `UserId` modification.
- **App.BLL** — `AppBLL` aggregates all services. Services receive the UoW and a BLL mapper. Business logic lives here.
- **App.DTO** — Versioned public API DTOs under `v1/`. Separate `CreateObjects/` and `UpdateObjects/` subdirectories. Each entity has its own manual mapper in `Mappers/`.
- **App.Resources** — `.resx` localization files for all domain entities and views. Three files per entity: `Entity.resx` (English/default), `Entity.et.resx`, `Entity.ru.resx`.
- **WebApp** — Three controller groups:
  - `ApiControllers/` — REST API (JWT Bearer, versioned `api/v{version}/...`)
  - `Areas/Admin/Controllers/` — MVC admin area (cookie auth, requires admin roles)
  - `Controllers/` — MVC user-facing pages (cookie auth)
  - `ApiControllers/Identity/AccountController` — login/logout/refresh-token using FreeIPA

### Manual mapper chain

Every entity has three mapper layers, each implementing `IMapper<TSource, TDest>` from `Base.Contracts`:

```
App.Domain.Entity
    ↕  App.DAL.EF/Mappers/EntityUOWMapper.cs       (Domain ↔ DAL.DTO)
App.DAL.DTO.Entity
    ↕  App.BLL/Mappers/EntityBLLMapper.cs           (DAL.DTO ↔ BLL.DTO)
App.BLL.DTO.Entity
    ↕  App.DTO/v1/Mappers/EntityMapper.cs           (BLL.DTO ↔ App.DTO)
App.DTO.v1.Entity
```

Navigation properties are nulled out in mappers to avoid circular references — only scalar fields and FK IDs are copied. Complex aggregated read results (e.g. `AssetViewModel`) live in `App.DAL.DTO/ViewModels/` and `App.BLL.DTO/ViewModels/` and use separate view model mappers.

### Authentication & authorization

Authentication is via **FreeIPA** (`ipa.lapikud.ee`), not ASP.NET Identity. On login, the app calls the IPA JSON-RPC API and **bidirectionally** syncs roles: adds roles present in IPA and removes roles from DB that are no longer in IPA. Roles used: `admins`, `helpdesk_db_admins`, `members`, `pixels`.

Both cookie auth (MVC) and JWT Bearer (API) are registered. API controllers use `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`.

IPA calls go through the `IIpaAuthClient` abstraction (`WebApp/ApiControllers/Identity/IIpaAuthClient.cs`), registered scoped in `Program.cs`. The real implementation `IpaAuthClient` wraps `FreeIPA.DotNet.IpaClient`. `AccountController` depends on `IIpaAuthClient` (not the concrete client), which lets tests substitute `FakeIpaAuthClient` instead of hitting the real IPA server. Both the API login and the MVC `Areas/Identity/Pages/Account/Login.cshtml.cs` resolve `IIpaAuthClient` from DI.

### Domain model overview

Assets are the core entity. Each asset can have:
- Many `CategoryAssets` (junction → `Category`)
- Many `LocationAssets` (junction → `Location`)
- Many `OwnerAssets` (junction → `Owner`)
- One `RemovedAssets` (soft-remove record)
- Many `AssetReservations` (user + time range)
  - `IsReturned` (bool): set via `POST Home/Returned`. `AssetReturned()` sets it true and trims `reservationTo` to `now` if it was in the future.
  - `IsReturned` gates all three availability queries: `IsAssetReservationAvailable`, `HasActiveOrFutureReservation`, and `GetAssetsReservedByUser` all filter `!IsReturned` — so a returned asset is immediately re-reservable.
  - Availability check applies a ±10-minute buffer around reservation times to prevent back-to-back conflicts.
- `SerialNumber` and `Barcode` on `Asset` are nullable strings (no min-length — optional identifiers). Both are included in the MVC overview search.
- `AssetReservation` extends `BaseEntityUser<AppUser>` (not `BaseEntity`) — `UserId` is inherited from the base class.

Physical location hierarchy: `Room` → `CupboardInRoom` → `Cupboard` → `LocationInCupboard` → `Location`.

### Adding a new entity

Each new entity touches all layers. Follow this checklist in order:

1. `App.Domain/Entity.cs` — domain class extending `BaseEntity`
2. `App.DAL.DTO/Entity.cs` — DAL DTO (mirrors domain, no EF attributes)
3. `App.DAL.EF/Mappers/EntityUOWMapper.cs` — implements `IMapper<DAL.DTO.Entity, Domain.Entity>`
4. `App.DAL.EF/AppDbContext.cs` — add `DbSet<Domain.Entity>`
5. `App.DAL.Contracts/IEntityRepository.cs` — repository interface extending `IBaseRepository<DAL.DTO.Entity>`
6. `App.DAL.EF/Repositories/EntityRepository.cs` — extends `BaseRepository<DAL.DTO.Entity, Domain.Entity>`
7. `App.DAL.Contracts/IAppUOW.cs` — add property `IEntityRepository EntityRepository`
8. `App.DAL.EF/AppUOW.cs` — wire up lazy-initialized repository
9. `App.BLL.DTO/Entity.cs` — BLL DTO
10. `App.BLL/Mappers/EntityBLLMapper.cs` — implements `IMapper<BLL.DTO.Entity, DAL.DTO.Entity>`
11. `App.BLL.Contracts/IEntityService.cs` — service interface extending `IBaseService<BLL.DTO.Entity>`
12. `App.BLL/Services/EntityService.cs` — extends `BaseService<BLL.DTO.Entity, DAL.DTO.Entity, IEntityRepository>`
13. `App.BLL.Contracts/IAppBLL.cs` — add property `IEntityService EntityService`
14. `App.BLL/AppBLL.cs` — wire up lazy-initialized service with its BLL mapper
15. `App.DTO/v1/Entity.cs` — public API DTO
16. `App.DTO/v1/Mappers/EntityMapper.cs` — implements `IMapper<App.DTO.v1.Entity, BLL.DTO.Entity>`
17. `WebApp/ApiControllers/EntitiesController.cs` — REST controller
18. `App.Resources/Domain/Entity.resx`, `Entity.et.resx`, `Entity.ru.resx` — localization strings
19. EF migration — `dotnet ef migrations add Add<Entity> ...`

### MVC reservation display rules

**`Views/AssetReservations/Index.cshtml`** (standalone reservations list): shows `IsReturned` as a column. Edit/Delete actions shown only when `userId == currentUser && reservationTo >= now`. Since `AssetReturned()` trims `reservationTo` to `now`, returned rows automatically lose their action links.

**`Views/Shared/_AssetListPartial.cshtml`** (overview "Assets Reserved By User" table): when `asset.Reserved == true`, shows "Change reservation time" + "Returned" + "Remove Reservation" buttons. After pressing "Returned", the asset disappears from the list because `GetAssetsReservedByUser` filters `!IsReturned`.

### Testing

All tests are self-contained — **no external PostgreSQL or FreeIPA server is required** to run `dotnet test`.

Integration tests (`App.Tests/IntegrationTests/`) use `CustomWebApplicationFactory<Program>`, which:
- Swaps the Npgsql `DbContextOptions<AppDbContext>` registration for a **SQLite in-memory** connection (`DataSource=:memory:`), kept open for the lifetime of the factory so the schema survives between calls.
- Replaces `IIpaAuthClient` with `FakeIpaAuthClient` (`App.Tests/IntegrationTests/FakeIpaAuthClient.cs`) so login succeeds without real IPA credentials. The fake returns a successful login and a canned `memberof_group` payload listing all four roles (`admins`, `members`, `pixels`, `helpdesk_db_admins`) — tweak `FakeIpaAuthClient.Groups` to simulate different role sets.
- Injects a test JWT signing key + issuer/audience via `AddInMemoryCollection`, so the test host does not depend on `.env`.
- Seeds deterministic rows (roles + categories, rooms, cupboards, locations, owners with fixed Guid IDs) in `SeedData` before tests run.

Unit tests (`App.Tests/UnitTests/`) use `TestDatabaseFixture.CreateContext()` — EF Core's **InMemory provider** with a fresh uniquely-named database per test (`AppTests_{Guid.NewGuid()}`). The fixture seeds the same entity shape as the integration factory and mocks `IUserNameResolver`. Repository and service layers both have coverage; unit tests exercise repositories directly against the in-memory context (not via Moq mocks of EF).

The `Program` class is declared `public partial` specifically to enable `WebApplicationFactory<Program>` in tests.

### Configuration

`appsettings.json` ships with **empty secrets** (connection strings and JWT key are blank). Real values must come from `HelpdeskDb/.env` (gitignored) for `dotnet run` and docker-compose. Tests do not read `.env` — they inject their own in-memory configuration.

First-time setup:

```bash
cp .env.example .env   # then edit .env with your values
```

`DotNetEnv` (v3.1.1) is called at the very top of `Program.cs` (`Env.TraversePath().Load()`) before the `WebApplicationBuilder` is created, so `.env` values are available to the configuration system as environment variable overrides.

Key variables in `.env`:

| Variable | Used by |
|---|---|
| `ConnectionStrings__DefaultConnection` | `dotnet run` local dev |
| `JWTSecurity__Key` | JWT signing (both local and Docker) |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | docker-compose db container |
| `DOCKER_DB_CONNECTION` | docker-compose webapp container (host = `lapikudhelpdesk-db-postgres`, matching the db service's `container_name`) |

JWT settings are under `JWTSecurity:` (Key, Issuer, Audience, ExpiresInSeconds, RefreshTokenExpiresInSeconds). Supported cultures and default culture are also in `appsettings.json`.

On startup, `DataSeeder` runs migrations and seeds roles and sample data (categories, rooms, cupboards, locations, owners) if the tables are empty.
