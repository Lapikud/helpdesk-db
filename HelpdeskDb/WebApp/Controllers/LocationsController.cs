using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class LocationsController : Controller
{
    private readonly IAppBLL _bll;

    public LocationsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Locations
    public async Task<IActionResult> Index()
    {
        var locs = await _bll.LocationService.AllAsync();
        return View(locs);
    }
    

    // GET: Locations/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Locations/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(App.BLL.DTO.Location location)
    {
        if (ModelState.IsValid)
        {
            await _bll.LocationService.AddAsync(location);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(location);
    }

    // GET: Locations/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await _bll.LocationService.FindAsync(id.Value);
        if (location == null)
        {
            return NotFound();
        }
        return View(location);
    }

    // POST: Locations/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, App.BLL.DTO.Location location)
    {
        if (id != location.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _bll.LocationService.UpdateAsync(location);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(location);
    }

    // GET: Locations/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var location = await _bll.LocationService.FindAsync(id.Value);
        if (location == null)
        {
            return NotFound();
        }

        return View(location);
    }

    // POST: Locations/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var locationAssets = (await _bll.LocationAssetsService.AllAsync()).Where(la => la.LocationId == id).ToList();
        var locationsInCupboards = (await _bll.LocationInCupboardService.AllAsync()).Where(lic => lic.LocationId == id).ToList();
        foreach (var la in locationAssets)
        {
            await _bll.LocationAssetsService.RemoveAsync(la.Id);
        }
        foreach (var lic in locationsInCupboards)
        {
            await _bll.LocationInCupboardService.RemoveAsync(lic.Id);
        }
        
        await _bll.LocationService.RemoveAsync(id);

        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}