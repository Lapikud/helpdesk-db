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
using App.DAL.EF;
using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class CategoryAssetsController : Controller
{
    private readonly IAppBLL _bll;

    public CategoryAssetsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: CategoryAssets
    public async Task<IActionResult> Index()
    {
        var res = await _bll.CategoryAssetsService.AllAsync();
        return View(res);
    }
    

    // GET: CategoryAssets/Create
    public async Task<IActionResult> Create()
    {
        var vm = new CategoryAssetCreateEditViewModel
        {
            AssetSelectList = await GetAssetSelectListAsync(),
            CategorySelectList = await GetCategorySelectListAsync(),
        };
        return View(vm);
    }

    // POST: CategoryAssets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryAssetCreateEditViewModel vm)
    {
        if (await _bll.CategoryAssetsService
                .GetCategoryAssetsByAssetId(vm.CategoryAssets.AssetId) == null &&
            ModelState.IsValid)
        {
            await _bll.CategoryAssetsService.AddAsync(vm.CategoryAssets);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.AssetSelectList = await GetAssetSelectListAsync(vm.CategoryAssets.AssetId);
        vm.CategorySelectList = await GetCategorySelectListAsync(vm.CategoryAssets.CategoryId);
        return View(vm);
    }

    // GET: CategoryAssets/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var categoryAssets = await _bll.CategoryAssetsService.FindAsync(id.Value);
        if (categoryAssets == null)
        {
            return NotFound();
        }

        var vm = new CategoryAssetCreateEditViewModel
        {
            AssetSelectList = await GetAssetSelectListAsync(categoryAssets.AssetId),
            CategorySelectList = await GetCategorySelectListAsync(),
            CategoryAssets = categoryAssets
        };
        return View(vm);
    }

    // POST: CategoryAssets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CategoryAssetCreateEditViewModel vm)
    {
        if (id != vm.CategoryAssets.Id)
        {
            return NotFound();
        }

        var changing = await _bll.CategoryAssetsService.FindAsync(vm.CategoryAssets.Id);

        var check = vm.CategoryAssets.AssetId == changing!.AssetId || 
                     await _bll.CategoryAssetsService.GetCategoryAssetsByAssetId(vm.CategoryAssets.AssetId) == null;

        if (ModelState.IsValid && check)
        {
            await _bll.CategoryAssetsService.UpdateAsync(vm.CategoryAssets);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.AssetSelectList = await GetAssetSelectListAsync(vm.CategoryAssets.AssetId);
        vm.CategorySelectList = await GetCategorySelectListAsync(vm.CategoryAssets.CategoryId);
        return View(vm);
    }

    // GET: CategoryAssets/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var categoryAssets = await _bll.CategoryAssetsService.FindAsync(id.Value);
        if (categoryAssets == null)
        {
            return NotFound();
        }

        return View(categoryAssets);
    }

    // POST: CategoryAssets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bll.CategoryAssetsService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
    {
        var assets = await _bll.AssetService.AllAsync();
        
        assets = assets.Where(a => !a.CategoryAssetsCollection!.Any() || a.Id == selectedValue).ToList();
        return new SelectList(assets,
            nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
    }

    private async Task<SelectList> GetCategorySelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.CategoryService.AllAsync(),
            nameof(Category.Id), nameof(Category.CategoryName), selectedValue);
    }
}