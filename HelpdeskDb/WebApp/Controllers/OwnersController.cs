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

[Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
public class OwnersController : Controller
{
    private readonly IAppBLL _bll;

    public OwnersController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Owners
    public async Task<IActionResult> Index()
    {
        var owners = await _bll.OwnerService.AllAsync();
        return View(owners);
    }

    // GET: Owners/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Owners/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Owner owner)
    {
        var owners = await _bll.OwnerService.AllAsync();
        if (owners.Any(c => c.OwnerName.Equals(owner.OwnerName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, App.Resources.Errors.ValidationErrors.OwnerNameExists);
            return View(owner);
        }
        if (ModelState.IsValid)
        {
            // owner.Id = Guid.NewGuid();
            await _bll.OwnerService.AddAsync(owner);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(owner);
    }

    // GET: Owners/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var owner = await _bll.OwnerService.FindAsync(id.Value);
        if (owner == null)
        {
            return NotFound();
        }
        return View(owner);
    }

    // POST: Owners/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Owner owner)
    {
        if (id != owner.Id)
        {
            return NotFound();
        }
        
        var owners = await _bll.OwnerService.AllAsync();
        if (owners.Any(c => c.OwnerName.Equals(owner.OwnerName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, App.Resources.Errors.ValidationErrors.OwnerNameExists);
            return View(owner);
        }

        if (ModelState.IsValid)
        {
            await _bll.OwnerService.UpdateAsync(owner);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(owner);
    }

    // GET: Owners/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var owner = await _bll.OwnerService.FindAsync(id.Value);
        if (owner == null)
        {
            return NotFound();
        }

        return View(owner);
    }

    // POST: Owners/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ownerAssets = (await _bll.OwnerAssetsService.AllAsync()).Where(ca => ca.OwnerId == id).ToList();
        foreach (var oa in ownerAssets)
        {
            await _bll.OwnerAssetsService.RemoveAsync(oa.Id);
        }
        await _bll.OwnerService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}