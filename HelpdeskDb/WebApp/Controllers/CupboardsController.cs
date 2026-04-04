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
public class CupboardsController : Controller
{
    private readonly IAppBLL _bll;

    public CupboardsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Cupboards
    public async Task<IActionResult> Index()
    {
        var cupboards = await _bll.CupboardService.AllAsync();
        return View(cupboards);
    }
    

    // GET: Cupboards/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Cupboards/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(App.BLL.DTO.Cupboard cupboard)
    {
        if (ModelState.IsValid)
        {
            // cupboard.Id = Guid.NewGuid();
            await _bll.CupboardService.AddAsync(cupboard);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(cupboard);
    }

    // GET: Cupboards/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cupboard = await _bll.CupboardService.FindAsync(id.Value);
        if (cupboard == null)
        {
            return NotFound();
        }
        return View(cupboard);
    }

    // POST: Cupboards/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, App.BLL.DTO.Cupboard cupboard)
    {
        if (id != cupboard.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _bll.CupboardService.UpdateAsync(cupboard);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(cupboard);
    }

    // GET: Cupboards/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cupboard = await _bll.CupboardService.FindAsync(id.Value);
        if (cupboard == null)
        {
            return NotFound();
        }

        return View(cupboard);
    }

    // POST: Cupboards/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var cupboardInRooms = (await _bll.CupboardInRoomService.AllAsync()).Where(ca => ca.CupboardId == id).ToList();
        var locationsInCupboards = (await _bll.LocationInCupboardService.AllAsync()).Where(ca => ca.CupboardId == id).ToList();
        foreach (var cir in cupboardInRooms)
        {
            await _bll.CupboardInRoomService.RemoveAsync(cir.Id);
        }
        foreach (var lic in locationsInCupboards)
        {
            await _bll.LocationInCupboardService.RemoveAsync(lic.Id);
        }
        
        await _bll.CupboardService.RemoveAsync(id);

        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}