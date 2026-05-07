using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
public class RemovedAssetsController : Controller
{
    private readonly IAppBLL _bll;

    public RemovedAssetsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: RemovedAssets
    public async Task<IActionResult> Index()
    {
        var res = await _bll.RemovedAssetsService.AllAsync();
        return View(res);
    }

    // GET: RemovedAssets/Create
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> Create()
    {
        var vm = new RemovedAssetCreateEditVm
        {
            AssetSelectList = await GetAssetSelectListAsync(),
        };
        return View(vm);
    }

    // POST: RemovedAssets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> Create(RemovedAssetCreateEditVm vm)
    {
        if (await _bll.RemovedAssetsService
                .GetRemovedAssetByAssetId(vm.RemovedAsset.AssetId) == null && ModelState.IsValid)
        {
            // removedAssets.Id = Guid.NewGuid();
            await _bll.RemovedAssetsService.AddAsync(vm.RemovedAsset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.AssetSelectList = await GetAssetSelectListAsync(vm.RemovedAsset.AssetId);
        return View(vm);
    }

    // GET: RemovedAssets/Edit/5
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var removedAssets = await _bll.RemovedAssetsService.FindAsync(id.Value);
        if (removedAssets == null)
        {
            return NotFound();
        }
        var vm = new RemovedAssetCreateEditVm
        {
            AssetSelectList = await GetAssetSelectListAsync(removedAssets.AssetId),
            RemovedAsset = removedAssets,
        };
        return View(vm);
    }

    // POST: RemovedAssets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> Edit(Guid id, RemovedAssetCreateEditVm vm)
    {
        if (id != vm.RemovedAsset.Id)
        {
            return NotFound();
        }

        var changing = await _bll.RemovedAssetsService.FindAsync(vm.RemovedAsset.Id);

        var check = vm.RemovedAsset.AssetId == changing!.AssetId || 
                    await _bll.RemovedAssetsService.GetRemovedAssetByAssetId(vm.RemovedAsset.AssetId) == null;
        
        if (ModelState.IsValid && check)
        {
            await _bll.RemovedAssetsService.UpdateAsync(vm.RemovedAsset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.AssetSelectList = await GetAssetSelectListAsync(vm.RemovedAsset.AssetId);
        return View(vm);
    }

    // GET: RemovedAssets/Delete/5
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var removedAssets = await _bll.RemovedAssetsService.FindAsync(id.Value);
        if (removedAssets == null)
        {
            return NotFound();
        }

        return View(removedAssets);
    }

    // POST: RemovedAssets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bll.RemovedAssetsService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
    {
        var assets = await _bll.AssetService.AllAsync();
        
        assets = assets.Where(a => !a.RemovedAssetsCollection!.Any() || a.Id == selectedValue).ToList();
        return new SelectList(assets,
            nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
    }
}