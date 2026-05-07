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
    public class RemovedAssetsController : Controller
    {
        private readonly AppDbContext _context;

        public RemovedAssetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RemovedAssets
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RemovedAssetsCollection.Include(r => r.Asset);
            return View(await appDbContext.ToListAsync());
        }

        // GET: RemovedAssets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var removedAssets = await _context.RemovedAssetsCollection
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (removedAssets == null)
            {
                return NotFound();
            }

            return View(removedAssets);
        }

        // GET: RemovedAssets/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = GetAssetSelectList();
            return View();
        }

        // POST: RemovedAssets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RemovedAssets removedAssets)
        {
            if (!await _context.RemovedAssetsCollection.AnyAsync(ra => ra.AssetId.Equals(removedAssets.AssetId)) && ModelState.IsValid)
            {
                removedAssets.Id = Guid.NewGuid();
                _context.Add(removedAssets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetId"] = GetAssetSelectList(removedAssets.AssetId);
            return View(removedAssets);
        }

        // GET: RemovedAssets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var removedAssets = await _context.RemovedAssetsCollection.FindAsync(id);
            if (removedAssets == null)
            {
                return NotFound();
            }
            ViewData["AssetId"] = GetAssetSelectList(removedAssets.AssetId);
            return View(removedAssets);
        }

        // POST: RemovedAssets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RemovedAssets removedAssets)
        {
            if (id != removedAssets.Id)
            {
                return NotFound();
            }

            var changing = await _context.RemovedAssetsCollection.FirstAsync(ra => ra.Id.Equals(removedAssets.Id));
            var check = removedAssets.AssetId == changing.AssetId ||
                        await _context.RemovedAssetsCollection
                            .FirstOrDefaultAsync(ca => ca.AssetId.Equals(removedAssets.AssetId)) == null;
            
            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(removedAssets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RemovedAssetsExists(removedAssets.Id))
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
            ViewData["AssetId"] = GetAssetSelectList(removedAssets.AssetId);
            return View(removedAssets);
        }

        // GET: RemovedAssets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var removedAssets = await _context.RemovedAssetsCollection
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (removedAssets == null)
            {
                return NotFound();
            }

            return View(removedAssets);
        }

        // POST: RemovedAssets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var removedAssets = await _context.RemovedAssetsCollection.FindAsync(id);
            if (removedAssets != null)
            {
                _context.RemovedAssetsCollection.Remove(removedAssets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RemovedAssetsExists(Guid id)
        {
            return _context.RemovedAssetsCollection.Any(e => e.Id == id);
        }
        
        private SelectList GetAssetSelectList(Guid? selectedValue = null)
        {
            var assets = _context.Assets.Include(a => a.RemovedAssetsCollection).ToList();
        
            assets = assets.Where(a => !a.RemovedAssetsCollection!.Any() || a.Id == selectedValue).ToList();
            return new SelectList(assets,
                nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
        }
    }
}
