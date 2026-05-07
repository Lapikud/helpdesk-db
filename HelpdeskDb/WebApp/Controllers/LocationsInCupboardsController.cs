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
public class LocationsInCupboardsController : Controller
{
    private readonly IAppBLL _bll;
    private readonly App.BLL.Mappers.LocationInCupboardBLLMapper _mapper =
        new App.BLL.Mappers.LocationInCupboardBLLMapper();

    public LocationsInCupboardsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: LocationsInCupboards
    public async Task<IActionResult> Index()
    {
        var res = await _bll.LocationInCupboardService.AllAsync();
        
        return View(res);
    }
    

    // GET: LocationsInCupboards/Create
    public async Task<IActionResult> Create()
    {
        var vm = new LocationInCupboardCreateEditVm
        {
            CupboardSelectList = await GetCupboardSelectListAsync(),
            LocationSelectList = await GetLocationSelectListAsync(),
        };
        return View(vm);
    }

    // POST: LocationsInCupboards/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationInCupboardCreateEditVm vm)
    {
        if (await _bll.LocationInCupboardService
                .GetLocationInCupboardByLocationId(vm.LocationInCupboard.LocationId) == null && ModelState.IsValid)
        {
            // locationInCupboard.Id = Guid.NewGuid();
            await _bll.LocationInCupboardService.AddAsync(vm.LocationInCupboard);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.LocationSelectList = await GetLocationSelectListAsync(vm.LocationInCupboard.LocationId);
        vm.CupboardSelectList = await GetCupboardSelectListAsync(vm.LocationInCupboard.CupboardId);
        return View(vm);
    }

    // GET: LocationsInCupboards/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var locationInCupboard = await _bll.LocationInCupboardService.FindAsync(id.Value);
        if (locationInCupboard == null)
        {
            return NotFound();
        }

        var vm = new LocationInCupboardCreateEditVm
        {
            CupboardSelectList = await GetCupboardSelectListAsync(),
            LocationSelectList = await GetLocationSelectListAsync(locationInCupboard.LocationId),
            LocationInCupboard = locationInCupboard,
        };
        return View(vm);
    }

    // POST: LocationsInCupboards/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, LocationInCupboardCreateEditVm vm)
    {
        if (id != vm.LocationInCupboard.Id)
        {
            return NotFound();
        }

        var changing = await _bll.LocationInCupboardService.FindAsync(vm.LocationInCupboard.Id);

        var check = vm.LocationInCupboard.LocationId == changing!.LocationId || 
                    await _bll.LocationInCupboardService.GetLocationInCupboardByLocationId(vm.LocationInCupboard.LocationId) == null;
        
        if (ModelState.IsValid && check)
        {
            await _bll.LocationInCupboardService.UpdateAsync(vm.LocationInCupboard);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.LocationSelectList = await GetLocationSelectListAsync(vm.LocationInCupboard.LocationId);
        vm.CupboardSelectList = await GetCupboardSelectListAsync(vm.LocationInCupboard.CupboardId);
        return View(vm);
    }

    // GET: LocationsInCupboards/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var locationInCupboard = await _bll.LocationInCupboardService.FindAsync(id.Value);
        if (locationInCupboard == null)
        {
            return NotFound();
        }

        return View(locationInCupboard);
    }

    // POST: LocationsInCupboards/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bll.LocationInCupboardService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetCupboardSelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.CupboardService.AllAsync(),
            nameof(Cupboard.Id), nameof(Cupboard.CodeName), selectedValue);
    }

    private async Task<SelectList> GetLocationSelectListAsync(Guid? selectedValue = null)
    {
        var locs = await _bll.LocationService.AllAsync();
        
        locs = locs.Where(l => !l.LocationsInCupboards!.Any() || l.Id == selectedValue).ToList();
        
        return new SelectList(locs,
            nameof(Location.Id), nameof(Location.LocationName), selectedValue);
    }
}