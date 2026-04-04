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
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,members,pixels,helpdesk_db_admins")]
public class CategoriesController : Controller
{
    private readonly IAppBLL _bll;

    public CategoriesController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _bll.CategoryService.AllAsync();
        return View(categories);
    }
    

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(App.BLL.DTO.Category category)
    {
        var categories = await _bll.CategoryService.AllAsync();
        if (categories.Any(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, App.Resources.Errors.ValidationErrors.CategoryNameExists);
            return View(category);
        }
        if (ModelState.IsValid)
        {
            category.Id = Guid.NewGuid();
            await _bll.CategoryService.AddAsync(category);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _bll.CategoryService.FindAsync(id.Value);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, App.BLL.DTO.Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }
        
        var categories = await _bll.CategoryService.AllAsync();
        if (categories.Any(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(string.Empty, App.Resources.Errors.ValidationErrors.CategoryNameExists);
            return View(category);
        }

        if (ModelState.IsValid)
        {
            await _bll.CategoryService.UpdateAsync(category);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _bll.CategoryService.FindAsync(id.Value);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var categoryAssets = (await _bll.CategoryAssetsService.AllAsync()).Where(ca => ca.CategoryId == id).ToList();
        foreach (var ca in categoryAssets)
        {
            await _bll.CategoryAssetsService.RemoveAsync(ca.Id);
        }
        await _bll.CategoryService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
}