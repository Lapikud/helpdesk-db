using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public class CategoryAssetsController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryAssetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CategoryAssets
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CategoryAssetsCollection.Include(c => c.Asset).Include(c => c.Category);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CategoryAssets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryAssets = await _context.CategoryAssetsCollection
                .Include(c => c.Asset)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryAssets == null)
            {
                return NotFound();
            }

            return View(categoryAssets);
        }

        // GET: CategoryAssets/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = GetAssetSelectList();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: CategoryAssets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryAssets categoryAssets)
        {
            if (!await _context.CategoryAssetsCollection.AnyAsync(ca => ca.AssetId.Equals(categoryAssets.AssetId)) &&
                ModelState.IsValid)
            {
                categoryAssets.Id = Guid.NewGuid();
                _context.Add(categoryAssets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AssetId"] = GetAssetSelectList(categoryAssets.AssetId);
            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "CategoryName", categoryAssets.CategoryId);
            return View(categoryAssets);
        }

        // GET: CategoryAssets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryAssets = await _context.CategoryAssetsCollection.FindAsync(id);
            if (categoryAssets == null)
            {
                return NotFound();
            }

            ViewData["AssetId"] = GetAssetSelectList(categoryAssets.AssetId);
            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "CategoryName", categoryAssets.CategoryId);
            return View(categoryAssets);
        }

        // POST: CategoryAssets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryAssets categoryAssets)
        {
            if (id != categoryAssets.Id)
            {
                return NotFound();
            }

            var changing = await _context.CategoryAssetsCollection.FirstAsync(ca => ca.Id.Equals(categoryAssets.Id));
            var check = categoryAssets.AssetId == changing.AssetId ||
                        await _context.CategoryAssetsCollection
                            .FirstOrDefaultAsync(ca => ca.AssetId.Equals(categoryAssets.AssetId)) == null;

            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(categoryAssets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryAssetsExists(categoryAssets.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["AssetId"] = GetAssetSelectList(categoryAssets.AssetId);
            ViewData["CategoryId"] =
                new SelectList(_context.Categories, "Id", "CategoryName", categoryAssets.CategoryId);
            return View(categoryAssets);
        }

        // GET: CategoryAssets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryAssets = await _context.CategoryAssetsCollection
                .Include(c => c.Asset)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var categoryAssets = await _context.CategoryAssetsCollection.FindAsync(id);
            if (categoryAssets != null)
            {
                _context.CategoryAssetsCollection.Remove(categoryAssets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryAssetsExists(Guid id)
        {
            return _context.CategoryAssetsCollection.Any(e => e.Id == id);
        }
        
        private SelectList GetAssetSelectList(Guid? selectedValue = null)
        {
            var assets = _context.Assets.Include(a => a.CategoryAssetsCollection).ToList();
        
            assets = assets.Where(a => !a.CategoryAssetsCollection!.Any() || a.Id == selectedValue).ToList();
            return new SelectList(assets,
                nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
        }
    }
}