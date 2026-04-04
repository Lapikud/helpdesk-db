using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApp.ViewModels;
using App.BLL.DTO;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class LocationAssetsController : Controller
{
    private readonly IAppBLL _bll;

    public LocationAssetsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: LocationAssets
    public async Task<IActionResult> Index()
    {
        var res = await _bll.LocationAssetsService.AllAsync();
        return View(res);
    }
    

    // GET: LocationAssets/Create
    public async Task<IActionResult> Create()
    {
        var vm = new LocationAssetCreateEditViewModel
        {
            AssetSelectList = await GetAssetSelectListAsync(),
            LocationSelectList = await GetLocationSelectListAsync(),
        };
        return View(vm);
    }

    // POST: LocationAssets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationAssetCreateEditViewModel vm)
    {
        if (await _bll.LocationAssetsService.GetLocationAssetsByAssetId(vm.LocationAssets.AssetId) == null &&
            ModelState.IsValid)
        {
            // locationAssets.Id = Guid.NewGuid();
            await _bll.LocationAssetsService.AddAsync(vm.LocationAssets);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.AssetSelectList = await GetAssetSelectListAsync(vm.LocationAssets.AssetId);
        vm.LocationSelectList = await GetLocationSelectListAsync(vm.LocationAssets.LocationId);
        return View(vm);
    }

    // GET: LocationAssets/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var locationAssets = await _bll.LocationAssetsService.FindAsync(id.Value);
        if (locationAssets == null)
        {
            return NotFound();
        }

        var vm = new LocationAssetCreateEditViewModel
        {
            AssetSelectList = await GetAssetSelectListAsync(locationAssets.AssetId),
            LocationSelectList = await GetLocationSelectListAsync(),
            LocationAssets = locationAssets,
        };
        return View(vm);
    }

    // POST: LocationAssets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LocationAssetCreateEditViewModel vm)
    {
        if (id != vm.LocationAssets.Id)
        {
            return NotFound();
        }

        var changing = await _bll.LocationAssetsService.FindAsync(vm.LocationAssets.Id);

        var check = vm.LocationAssets.AssetId == changing!.AssetId ||
                    await _bll.LocationAssetsService.GetLocationAssetsByAssetId(vm.LocationAssets.AssetId) == null;

        if (ModelState.IsValid && check)
        {
            await _bll.LocationAssetsService.UpdateAsync(vm.LocationAssets);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.AssetSelectList = await GetAssetSelectListAsync(vm.LocationAssets.AssetId);
        vm.LocationSelectList = await GetLocationSelectListAsync(vm.LocationAssets.LocationId);
        return View(vm);
    }

    // GET: LocationAssets/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var locationAssets = await _bll.LocationAssetsService.FindAsync(id.Value);
        if (locationAssets == null)
        {
            return NotFound();
        }

        return View(locationAssets);
    }

    // POST: LocationAssets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bll.LocationAssetsService.RemoveAsync(id);

        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
    {
        var assets = await _bll.AssetService.AllAsync();
        Console.WriteLine(selectedValue);
        
        assets = assets.Where(a => !a.LocationsAssetsCollection!.Any() || a.Id == selectedValue).ToList();
        return new SelectList(assets,
            nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
    }

    private async Task<SelectList> GetLocationSelectListAsync(Guid? selectedValue = null)
    {
        var locs = await _bll.LocationService.AllAsync();
        
        return new SelectList(locs,
            nameof(Location.Id), nameof(Location.LocationName), selectedValue);
    }
}