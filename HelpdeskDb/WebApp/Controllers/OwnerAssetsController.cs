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

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class OwnerAssetsController : Controller
{
    private readonly IAppBLL _bll;

    public OwnerAssetsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: OwnerAssets
    public async Task<IActionResult> Index()
    {
        var res = await _bll.OwnerAssetsService.AllAsync();
        return View(res);
    }

    // GET: OwnerAssets/Create
    public async Task<IActionResult> Create()
    {
        var vm = new OwnerAssetCreateEditVm
        {
            AssetSelectList = await GetAssetSelectListAsync(),
            OwnerSelectList = await GetOwnerSelectListAsync(),
        };
        return View(vm);
    }

    // POST: OwnerAssets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OwnerAssetCreateEditVm vm)
    {
        if (await _bll.OwnerAssetsService
                .GetOwnerAssetsByAssetId(vm.OwnerAsset.AssetId) == null && ModelState.IsValid)
        {
            // ownerAssets.Id = Guid.NewGuid();
            await _bll.OwnerAssetsService.AddAsync(vm.OwnerAsset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.AssetSelectList = await GetAssetSelectListAsync(vm.OwnerAsset.AssetId);
        vm.OwnerSelectList = await GetOwnerSelectListAsync(vm.OwnerAsset.OwnerId);
        return View(vm);
    }

    // GET: OwnerAssets/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ownerAssets = await _bll.OwnerAssetsService.FindAsync(id.Value);
        if (ownerAssets == null)
        {
            return NotFound();
        }
        var vm = new OwnerAssetCreateEditVm
        {
            AssetSelectList = await GetAssetSelectListAsync(ownerAssets.AssetId),
            OwnerSelectList = await GetOwnerSelectListAsync(),
            OwnerAsset = ownerAssets,
        };
        return View(vm);
    }

    // POST: OwnerAssets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OwnerAssetCreateEditVm vm)
    {
        if (id != vm.OwnerAsset.Id)
        {
            return NotFound();
        }

        var changing = await _bll.OwnerAssetsService.FindAsync(vm.OwnerAsset.Id);

        var check = vm.OwnerAsset.AssetId == changing!.AssetId || 
                    await _bll.OwnerAssetsService.GetOwnerAssetsByAssetId(vm.OwnerAsset.AssetId) == null;
        
        if (ModelState.IsValid && check)
        {
            await _bll.OwnerAssetsService.UpdateAsync(vm.OwnerAsset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.AssetSelectList = await GetAssetSelectListAsync(vm.OwnerAsset.AssetId);
        vm.OwnerSelectList = await GetOwnerSelectListAsync(vm.OwnerAsset.OwnerId);
        return View(vm);
    }

    // GET: OwnerAssets/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var ownerAssets = await _bll.OwnerAssetsService.FindAsync(id.Value);
        if (ownerAssets == null)
        {
            return NotFound();
        }

        return View(ownerAssets);
    }

    // POST: OwnerAssets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bll.OwnerAssetsService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
    {
        var assets = await _bll.AssetService.AllAsync();
        
        assets = assets.Where(a => !a.OwnerAssets!.Any() || a.Id == selectedValue).ToList();
        return new SelectList(assets,
            nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
    }
    
    private async Task<SelectList> GetOwnerSelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.OwnerService.AllAsync(),
            nameof(Owner.Id), nameof(Owner.OwnerName), selectedValue);
    }
}