dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef

dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool update --global dotnet-aspnet-codegenerator

# Run the application

cd HelpdeskDb
dotnet run --project .\WebApp\WebApp.csproj

```sh
cd HelpdeskDb
```

```sh
dotnet ef migrations add InitialCreate --project App.DAL.EF --startup-project WebApp --context AppDbContext
```

```sh
dotnet ef database --project App.DAL.EF --startup-project WebApp drop
```

```sh
dotnet ef database --project App.DAL.EF --startup-project WebApp update
```

```sh
cd WebApp
```

- MVC controllers

```sh
dotnet aspnet-codegenerator controller -name AssetsController -actions -m Domain.Asset -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CategoriesController -actions -m Domain.Category -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CategoryAssetsController -actions -m Domain.CategoryAssets -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CupboardsController -actions -m Domain.Cupboard -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CupboardsInRoomsController -actions -m Domain.CupboardInRoom -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationsController -actions -m Domain.Location -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationAssetsController -actions -m Domain.LocationAssets -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationsInCupboardsController -actions -m Domain.LocationInCupboard -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OwnersController -actions -m Domain.OwnersController -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OwnerAssetsController -actions -m Domain.OwnerAssets -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RemovedAssetsController -actions -m Domain.RemovedAssets -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RoomsController -actions -m Domain.Room -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name AssetReservationsController -actions -m Domain.AssetReservation -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

```sh
dotnet aspnet-codegenerator controller -name AssetsController -actions -m App.Domain.Asset -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CategoriesController -actions -m App.Domain.Category -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CategoryAssetsController -actions -m App.Domain.CategoryAssets -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CupboardsController -actions -m App.Domain.Cupboard -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name CupboardsInRoomsController -actions -m App.Domain.CupboardInRoom -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationsController -actions -m App.Domain.Location -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationAssetsController -actions -m App.Domain.LocationAssets -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name LocationsInCupboardsController -actions -m App.Domain.LocationInCupboard -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OwnersController -actions -m App.Domain.Owner -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OwnerAssetsController -actions -m App.Domain.OwnerAssets -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RemovedAssetsController -actions -m App.Domain.RemovedAssets -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RoomsController -actions -m App.Domain.Room -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name AssetReservationsController -actions -m App.Domain.AssetReservation -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

```sh
dotnet aspnet-codegenerator controller -name RefreshTokensController -actions -m  App.Domain.Identity.AppRefreshToken -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

```sh
dotnet aspnet-codegenerator controller -name UsersController -actions -m  App.Domain.Identity.AppUser -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

```sh
dotnet aspnet-codegenerator controller -name RolesController -actions -m  App.Domain.Identity.AppRole -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

```sh
dotnet aspnet-codegenerator controller -name UserRolesController -actions -m  App.Domain.Identity.AppUserRole -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
```

- Identity

```sh
dotnet aspnet-codegenerator identity -dc App.DAL.EF.AppDbContext -f
```

- Api controllers

```sh
dotnet aspnet-codegenerator controller -name AssetsController -m App.Domain.Asset -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name CategoriesController -m App.Domain.Category -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name CategoryAssetsController -m App.Domain.CategoryAssets -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name CupboardsController -m App.Domain.Cupboard -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name CupboardsInRoomsController -m App.Domain.CupboardInRoom -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name LocationsController -m App.Domain.Location -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name LocationAssetsController -m App.Domain.LocationAssets -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name LocationsInCupboardsController -m App.Domain.LocationInCupboard -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name OwnersController -m App.Domain.Owner -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name OwnerAssetsController -m App.Domain.OwnerAssets -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name RoomsController -m App.Domain.Room -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name RemovedAssetsController -m App.Domain.RemovedAssets -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name UserAssetsController -m App.Domain.UserAssets -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name AssetReservationsController -m App.Domain.AssetReservation -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f

dotnet aspnet-codegenerator controller -name RolesController -m App.Domain.Identity.AppRole -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name UserRolesController -m App.Domain.Identity.AppUserRole -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name UsersController -m App.Domain.Identity.AppUser -dc App.DAL.EF.AppDbContext -outDir ApiControllers -api --useAsyncActions -f

```
