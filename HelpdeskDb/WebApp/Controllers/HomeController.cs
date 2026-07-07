using System.Diagnostics;
using App.BLL.Contracts;
using App.BLL.DTO;
using App.BLL.DTO.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;
using Base.Contracts;
using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebApp.Controllers;

// [Authorize(Roles = "admin,lapikud")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAppBLL _bll;
    private readonly IUserNameResolver _userNameResolver;

    public HomeController(ILogger<HomeController> logger, IAppBLL bll, IUserNameResolver userNameResolver)
    {
        _logger = logger;
        _bll = bll;
        _userNameResolver = userNameResolver;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Overview(string searchTerm, bool isAvailableAssetsOpen = false,
        bool isAssetsReservedByUserOpen = false)
    {
        var availableAssets = await _bll.AssetService.GetAvailableAssets();
        var assetsReservedByUser = await _bll.AssetService.GetAssetsReservedByUser(User.GetUserId());

        if (!string.IsNullOrEmpty(searchTerm))
        {
            availableAssets = availableAssets.Where(a =>
                a.AssetName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.OwnerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (a.SerialNumber != null && a.SerialNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (a.Barcode != null && a.Barcode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            isAvailableAssetsOpen = true;
        }

        var vm = new AssetsOverviewViewModel
        {
            AvailableAssets = availableAssets,
            AssetsReservedByUser = assetsReservedByUser
        };

        ViewBag.IsAvailableAssetsOpen = isAvailableAssetsOpen;
        ViewBag.IsAssetsReservedByUserOpen = isAssetsReservedByUserOpen;

        return View(vm);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Set Language
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            }
        );
        // Only redirect back to a local path; a null / off-site returnUrl would otherwise throw
        // (500) or enable an open redirect.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Home/CreateNewAsset
    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    public async Task<IActionResult> CreateNewAsset()
    {
        var vm = new AssetCreateEditViewModel()
        {
            Categories = await GetCategorySelectListAsync(),
            Owners = await GetOwnerSelectListAsync(),
            Locations = await GetLocationSelectListAsync(),
        };
        return View(vm);
    }

    // POST: Home/CreateNewAsset
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNewAsset(AssetCreateEditViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var newAsset = await _bll.AssetService.CreateNewAsset(vm.Asset.AssetName, vm.Asset.Comment, vm.Asset.SerialNumber, vm.Asset.Barcode);

            await _bll.CategoryAssetsService.CreateNewCategoryAsset(newAsset.Id, vm.SelectedCategoryId);

            await _bll.OwnerAssetsService.CreateNewOwnerAsset(newAsset.Id, vm.SelectedOwnerId);

            await _bll.LocationAssetsService.CreateNewLocationAsset(newAsset.Id, vm.SelectedLocationId);

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Overview), new { isNotTakenOpen = true });
        }

        vm.Categories = await GetCategorySelectListAsync();
        vm.Owners = await GetOwnerSelectListAsync();
        vm.Locations = await GetLocationSelectListAsync();

        return View(vm);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await SetUpAsset(id.Value);

        if (asset == null)
        {
            return NotFound();
        }

        var vm = new AssetCreateEditViewModel()
        {
            Asset = asset,
            SelectedCategoryId = asset.CategoryAssetsCollection!.First().CategoryId,
            SelectedLocationId = asset.LocationsAssetsCollection!.First().LocationId,
            SelectedOwnerId = asset.OwnerAssets!.First().OwnerId,
            Categories = await GetCategorySelectListAsync(),
            Owners = await GetOwnerSelectListAsync(),
            Locations = await GetLocationSelectListAsync(),
        };
        return View(vm);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id,
        AssetCreateEditViewModel assetCreateEditViewModel,
        Guid catId, Guid selectedCategoryId,
        Guid ownId, Guid selectedOwnerId,
        Guid locId, Guid selectedLocationId)
    {
        if (id != assetCreateEditViewModel.Asset.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if ((await _bll.CategoryAssetsService.AllAsync()).FirstOrDefault(ca =>
                    ca.AssetId == id && ca.CategoryId == selectedCategoryId) == null)
            {
                await _bll.CategoryAssetsService.CreateNewCategoryAsset(id, selectedCategoryId);
            }

            if ((await _bll.OwnerAssetsService.AllAsync()).FirstOrDefault(oa =>
                    oa.AssetId == id && oa.OwnerId == selectedOwnerId) == null)
            {
                await _bll.OwnerAssetsService.CreateNewOwnerAsset(id, selectedOwnerId);
            }

            if ((await _bll.LocationAssetsService.AllAsync()).FirstOrDefault(la =>
                    la.AssetId == id && la.LocationId == selectedCategoryId) == null)
            {
                await _bll.LocationAssetsService.CreateNewLocationAsset(id, selectedLocationId);
            }

            await _bll.AssetService.UpdateAsync(assetCreateEditViewModel.Asset);
            await _bll.CategoryAssetsService.UpdateCategoryOfAsset(catId, selectedCategoryId);
            await _bll.OwnerAssetsService.UpdateOwnerOfAsset(ownId, selectedOwnerId);
            await _bll.LocationAssetsService.UpdateLocationOfAsset(locId, selectedLocationId);

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Overview));
        }

        return View(assetCreateEditViewModel);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    public async Task<IActionResult> RemoveReservation(Guid id)
    {
        var asset = await _bll.AssetService.FindAsync(id);

        if (asset == null)
        {
            return NotFound();
        }

        await _bll.AssetReservationService.RemoveAssetReservation(User.GetUserId(), asset.Id);
        await _bll.SaveChangesAsync();

        return RedirectToAction(nameof(Overview), new { isTakenOpen = true });
    }
    
    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    public async Task<IActionResult> Returned(Guid id)
    {
        var asset = await _bll.AssetService.FindAsync(id);

        if (asset == null)
        {
            return NotFound();
        }

        await _bll.AssetReservationService.AssetReturned(User.GetUserId(), asset.Id);
        await _bll.SaveChangesAsync();

        return RedirectToAction(nameof(Overview), new { isTakenOpen = true });
    }


    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    public async Task<IActionResult> Remove(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await SetUpAsset(id.Value);

        if (asset == null)
        {
            return NotFound();
        }

        var vm = new AssetRemoveViewModel()
        {
            Asset = asset,
            SelectedCategoryId = asset.CategoryAssetsCollection!.First().CategoryId,
            SelectedLocationId = asset.LocationsAssetsCollection!.First().LocationId,
            SelectedOwnerId = asset.OwnerAssets!.First().OwnerId,
            Categories = await GetCategorySelectListAsync(),
            Owners = await GetOwnerSelectListAsync(),
            Locations = await GetLocationSelectListAsync(),
        };
        return View(vm);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid id,
        AssetRemoveViewModel vm)
    {
        if (id != vm.Asset.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var removedAsset = await _bll.RemovedAssetsService.CreateNewRemovedAsset(vm.Asset.Id, vm.Comment);
            if (removedAsset == null)
            {
                return NotFound();
            }

            await _bll.SaveChangesAsync();

            _logger.LogInformation("Asset {AssetId} removed by {User} with comment: {Comment}",
                vm.Asset.Id, _userNameResolver.CurrentUserName, vm.Comment);

            return RedirectToAction(nameof(Overview));
        }

        return View(vm);
    }

    // Reserve asset
    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    public async Task<IActionResult> Reserve(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await SetUpAsset(id.Value);

        if (asset == null)
        {
            return NotFound();
        }

        var assetReservation = new AssetReservation()
        {
            Id = Guid.NewGuid(),
            Asset = asset,
            AssetId = asset.Id,
            UserId = User.GetUserId(),
            User = new AppUser() { Username = _userNameResolver.CurrentUserName, Id = User.GetUserId() },
            ReservationFrom = DateTime.Now,
            ReservationTo = DateTime.Now,
        };

        var vm = new AssetReservationCreateEditVm()
        {
            AssetReservation = assetReservation,
            SelectedUserId = assetReservation.UserId,
            SelectedAssetId = assetReservation.AssetId,
            AssetSelectList = await GetAssetSelectListAsync(),
            UserSelectList = GetUserSelectList(),
        };
        return View(vm);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(Guid id,
        AssetReservationCreateEditVm vm)
    {
        if (id != vm.AssetReservation.AssetId)
        {
            return NotFound();
        }
        
        if (await _bll.AssetReservationService.HasActiveOrFutureReservation(id, User.GetUserId()))
        {
            ModelState.AddModelError(nameof(AssetReservation.User),
                "You already have an active reservation for this asset.");
        }

        var reservationFrom = vm.AssetReservation.ReservationFrom.ToUniversalTime();
        var reservationTo = vm.AssetReservation.ReservationTo.ToUniversalTime();

        if (!await _bll.AssetReservationService.IsAssetReservationAvailable(vm.AssetReservation.AssetId,
                reservationFrom, reservationTo))
        {
            ModelState.AddModelError(nameof(AssetReservation.ReservationFrom),
                "Reservation for that time is unavailable.");
            ModelState.AddModelError(nameof(AssetReservation.ReservationTo),
                "Reservation for that time is unavailable.");
        }

        if (ModelState.IsValid && await _bll.AssetReservationService.IsAssetReservationAvailable(
                vm.AssetReservation.AssetId,
                reservationFrom,
                reservationTo))
        {
            await _bll.AssetReservationService.UserReserveAsset(vm.AssetReservation.UserId,
                vm.AssetReservation.AssetId, reservationFrom,
                reservationTo);

            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Overview));
        }

        vm.AssetSelectList = await GetAssetSelectListAsync();
        vm.UserSelectList = GetUserSelectList();

        return View(vm);
    }


    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    public async Task<IActionResult> ChangeReservationTime(Guid? reservationId)
    {
        if (reservationId == null)
        {
            return NotFound();
        }

        var reservation = await _bll.AssetReservationService
            .FindAsync(reservationId.Value);

        if (reservation == null)
        {
            return NotFound();
        }

        reservation.ReservationFrom = reservation.ReservationFrom.ToLocalTime();
        reservation.ReservationTo = reservation.ReservationTo.ToLocalTime();

        var vm = new AssetReservationCreateEditVm()
        {
            AssetReservation = reservation,
            SelectedUserId = reservation.UserId,
            SelectedAssetId = reservation.AssetId,
            AssetSelectList = await GetAssetSelectListAsync(),
            UserSelectList = GetUserSelectList(),
        };
        return View(vm);
    }

    [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeReservationTime(Guid reservationId,
        AssetReservationCreateEditVm vm)
    {
        if (!reservationId.Equals(vm.AssetReservation.Id) || !vm.SelectedUserId.Equals(User.GetUserId()))
        {
            return NotFound();
        }
        
        var existing = await _bll.AssetReservationService.FindAsync(reservationId);
        if (existing == null || !existing.UserId.Equals(User.GetUserId()))
        {
            return NotFound();
        }

        var reservationFrom = vm.AssetReservation.ReservationFrom.ToUniversalTime();
        var reservationTo = vm.AssetReservation.ReservationTo.ToUniversalTime();

        // Check availability excluding the current reservation ID so it doesn't block itself
        if (!await _bll.AssetReservationService.IsAssetReservationAvailable(
                existing.AssetId, reservationFrom, reservationTo, reservationId))
        {
            ModelState.AddModelError(nameof(AssetReservation.ReservationFrom),
                "Reservation for that time is unavailable.");
            ModelState.AddModelError(nameof(AssetReservation.ReservationTo),
                "Reservation for that time is unavailable.");
        }

        if (ModelState.IsValid)
        {
            existing.ReservationFrom = reservationFrom;
            existing.ReservationTo = reservationTo;
            await _bll.AssetReservationService.UpdateAsync(existing);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Overview), new { isAssetsReservedByUserOpen = true });
        }

        vm.AssetSelectList = await GetAssetSelectListAsync();
        vm.UserSelectList = GetUserSelectList();
        return View(vm);
    }

    private async Task<Asset?> SetUpAsset(Guid assetId)
    {
        var asset = await _bll.AssetService.FindAsync(assetId); //await _db.GetAssetById(id.Value);

        var categoryAssets =
            await _bll.CategoryAssetsService
                .GetCategoryAssetsByAssetId(assetId); // _db.GetCategoryAssetByAssetId(id.Value).Result;
        var ownerAssets =
            await _bll.OwnerAssetsService
                .GetOwnerAssetsByAssetId(assetId); // _db.GetOwnerAssetByAssetId(id.Value).Result;
        var locationAssets =
            await _bll.LocationAssetsService
                .GetLocationAssetsByAssetId(assetId); // _db.GetLocationAssetByAssetId(id.Value).Result;

        if (asset == null)
        {
            return null;
        }

        if (categoryAssets == null)
        {
            var category = (await _bll.CategoryService.AllAsync()).FirstOrDefault();
            if (category == null) return null;

            categoryAssets = new CategoryAssets()
            {
                Id = Guid.NewGuid(),
                CategoryId = category.Id,
                AssetId = assetId,
                Comment = "blank"
            };
        }

        if (ownerAssets == null)
        {
            var owner = (await _bll.OwnerService.AllAsync()).FirstOrDefault();
            if (owner == null) return null;
            ownerAssets = new OwnerAssets()
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                AssetId = assetId,
            };
        }

        if (locationAssets == null)
        {
            var location = (await _bll.LocationService.AllAsync()).FirstOrDefault();
            if (location == null) return null;
            locationAssets = new LocationAssets()
            {
                Id = Guid.NewGuid(),
                LocationId = location.Id,
                AssetId = assetId,
            };
        }

        asset.CategoryAssetsCollection = new List<CategoryAssets> { categoryAssets };
        asset.OwnerAssets = new List<OwnerAssets> { ownerAssets };
        asset.LocationsAssetsCollection = new List<LocationAssets> { locationAssets };

        return asset;
    }

    private async Task<SelectList> GetCategorySelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.CategoryService.AllAsync(),
            nameof(Category.Id), nameof(Category.CategoryName), selectedValue);
    }


    private async Task<SelectList> GetLocationSelectListAsync(Guid? selectedValue = null)
    {
        var locs = await _bll.LocationService.AllAsync();

        return new SelectList(locs,
            nameof(Location.Id), nameof(Location.LocationName), selectedValue);
    }

    private async Task<SelectList> GetOwnerSelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.OwnerService.AllAsync(),
            nameof(Owner.Id), nameof(Owner.OwnerName), selectedValue);
    }

    private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.AssetService.AllAsync(),
            nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
    }

    private SelectList GetUserSelectList(Guid? selectedValue = null)
    {
        return new SelectList(
            new[]
            {
                new
                {
                    Id = User.GetUserId(),
                    Username = User.Identity?.Name
                }
            },
            nameof(AppUser.Id), nameof(AppUser.Username), selectedValue);
    }
}