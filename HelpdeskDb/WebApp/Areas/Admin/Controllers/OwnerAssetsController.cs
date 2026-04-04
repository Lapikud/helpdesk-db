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
    public class OwnerAssetsController : Controller
    {
        private readonly AppDbContext _context;

        public OwnerAssetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: OwnerAssets
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.OwnerAssets.Include(o => o.Asset).Include(o => o.Owner);
            return View(await appDbContext.ToListAsync());
        }

        // GET: OwnerAssets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownerAssets = await _context.OwnerAssets
                .Include(o => o.Asset)
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ownerAssets == null)
            {
                return NotFound();
            }

            return View(ownerAssets);
        }

        // GET: OwnerAssets/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = GetAssetSelectList();
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "OwnerName");
            return View();
        }

        // POST: OwnerAssets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OwnerAssets ownerAssets)
        {
            if (!await _context.OwnerAssets.AnyAsync(oa => oa.AssetId.Equals(ownerAssets.AssetId)) && ModelState.IsValid)
            {
                ownerAssets.Id = Guid.NewGuid();
                _context.Add(ownerAssets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetId"] = GetAssetSelectList(ownerAssets.AssetId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "OwnerName", ownerAssets.OwnerId);
            return View(ownerAssets);
        }

        // GET: OwnerAssets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownerAssets = await _context.OwnerAssets.FindAsync(id);
            if (ownerAssets == null)
            {
                return NotFound();
            }
            ViewData["AssetId"] = GetAssetSelectList(ownerAssets.AssetId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "OwnerName", ownerAssets.OwnerId);
            return View(ownerAssets);
        }

        // POST: OwnerAssets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OwnerAssets ownerAssets)
        {
            if (id != ownerAssets.Id)
            {
                return NotFound();
            }

            var changing = await _context.OwnerAssets.FirstAsync(ca => ca.Id.Equals(ownerAssets.Id));
            var check = ownerAssets.AssetId == changing.AssetId ||
                        await _context.OwnerAssets
                            .FirstOrDefaultAsync(ca => ca.AssetId.Equals(ownerAssets.AssetId)) == null;
            
            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(ownerAssets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnerAssetsExists(ownerAssets.Id))
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
            ViewData["AssetId"] = GetAssetSelectList(ownerAssets.AssetId);
            ViewData["OwnerId"] = new SelectList(_context.Owners, "Id", "OwnerName", ownerAssets.OwnerId);
            return View(ownerAssets);
        }

        // GET: OwnerAssets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ownerAssets = await _context.OwnerAssets
                .Include(o => o.Asset)
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var ownerAssets = await _context.OwnerAssets.FindAsync(id);
            if (ownerAssets != null)
            {
                _context.OwnerAssets.Remove(ownerAssets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OwnerAssetsExists(Guid id)
        {
            return _context.OwnerAssets.Any(e => e.Id == id);
        }
        
        private SelectList GetAssetSelectList(Guid? selectedValue = null)
        {
            var assets = _context.Assets.Include(a => a.OwnerAssets).ToList();
            Console.WriteLine(selectedValue);
        
            assets = assets.Where(a => !a.OwnerAssets!.Any() || a.Id == selectedValue).ToList();
            return new SelectList(assets,
                nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
        }
    }
}
