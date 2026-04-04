using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.BLL.DTO;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class AssetsController : Controller
{
    private readonly IAppBLL _bll;

    public AssetsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Assets
    public async Task<IActionResult> Index()
    {
        var assets = await _bll.AssetService.GetAllNotRemovedAssets();
        return View(assets);
    }
    

    // GET: Assets/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Assets/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(App.BLL.DTO.Asset asset)
    {
        if (ModelState.IsValid)
        {
            // asset.Id = Guid.NewGuid();
            await _bll.AssetService.AddAsync(asset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(asset);
    }

    // GET: Assets/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await _bll.AssetService.FindAsync(id.Value);
        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    // POST: Assets/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, App.BLL.DTO.Asset asset)
    {
        if (id != asset.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _bll.AssetService.UpdateAsync(asset);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(asset);
    }

    // GET: Assets/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asset = await _bll.AssetService.FindAsync(id.Value);
        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    // POST: Assets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var locationAssets = (await _bll.LocationAssetsService.AllAsync()).Where(la => la.AssetId == id).ToList();
        var categoryAssets = (await _bll.CategoryAssetsService.AllAsync()).Where(ca => ca.AssetId == id).ToList();
        var ownerAssets = (await _bll.OwnerAssetsService.AllAsync()).Where(oa => oa.AssetId == id).ToList();
        var removedAssets = (await _bll.RemovedAssetsService.AllAsync()).Where(ra => ra.AssetId == id).ToList();
        var assetReservations = (await _bll.AssetReservationService.AllAsync()).Where(ar => ar.AssetId == id).ToList();
        foreach (var la in locationAssets)
        {
            await _bll.LocationAssetsService.RemoveAsync(la.Id);
        }
        foreach (var ca in categoryAssets)
        {
            await _bll.CategoryAssetsService.RemoveAsync(ca.Id);
        }
        foreach (var oa in ownerAssets)
        {
            await _bll.OwnerAssetsService.RemoveAsync(oa.Id);
        }
        foreach (var ra in removedAssets)
        {
            await _bll.RemovedAssetsService.RemoveAsync(ra.Id);
        }
        foreach (var ar in assetReservations)
        {
            await _bll.AssetReservationService.RemoveAsync(ar.Id, ar.UserId);
        }
        await _bll.AssetService.RemoveAsync(id);
        
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}