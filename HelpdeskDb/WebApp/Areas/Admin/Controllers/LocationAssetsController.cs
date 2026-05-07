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
    public class LocationAssetsController : Controller
    {
        private readonly AppDbContext _context;

        public LocationAssetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LocationAssets
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LocationAssetsCollection
                .Include(l => l.Asset)
                .Include(l => l.Location);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LocationAssets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationAssets = await _context.LocationAssetsCollection
                .Include(l => l.Asset)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (locationAssets == null)
            {
                return NotFound();
            }

            return View(locationAssets);
        }

        // GET: LocationAssets/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = GetAssetSelectList();
            ViewData["LocationId"] = new SelectList(_context.Locations.OrderBy(la => la.LocationName),
                "Id", "LocationName");
            return View();
        }

        // POST: LocationAssets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocationAssets locationAssets)
        {
            if (!await _context.LocationAssetsCollection.AnyAsync(la => la.AssetId.Equals(locationAssets.AssetId)) &&
                ModelState.IsValid)
            {
                locationAssets.Id = Guid.NewGuid();
                _context.Add(locationAssets);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AssetId"] = GetAssetSelectList(locationAssets.AssetId);
            ViewData["LocationId"] =
                new SelectList(_context.Locations, "Id", "LocationName", locationAssets.LocationId);
            return View(locationAssets);
        }

        // GET: LocationAssets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationAssets = await _context.LocationAssetsCollection.FindAsync(id);
            if (locationAssets == null)
            {
                return NotFound();
            }

            ViewData["AssetId"] = GetAssetSelectList(locationAssets.AssetId);
            ViewData["LocationId"] =
                new SelectList(_context.Locations, "Id", "LocationName", locationAssets.LocationId);
            return View(locationAssets);
        }

        // POST: LocationAssets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, LocationAssets locationAssets)
        {
            if (id != locationAssets.Id)
            {
                return NotFound();
            }

            var changing = await _context.LocationAssetsCollection.FirstAsync(la => la.Id.Equals(locationAssets.Id));
            var check = locationAssets.AssetId == changing.AssetId ||
                        await _context.LocationAssetsCollection.FirstOrDefaultAsync(la =>
                            la.AssetId.Equals(locationAssets.AssetId)) == null;

            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(locationAssets);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationAssetsExists(locationAssets.Id))
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

            ViewData["AssetId"] = GetAssetSelectList(locationAssets.AssetId);
            ViewData["LocationId"] =
                new SelectList(_context.Locations, "Id", "LocationName", locationAssets.LocationId);
            return View(locationAssets);
        }

        // GET: LocationAssets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationAssets = await _context.LocationAssetsCollection
                .Include(l => l.Asset)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (locationAssets == null)
            {
                return NotFound();
            }

            return View(locationAssets);
        }

        // POST: LocationAssets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var locationAssets = await _context.LocationAssetsCollection.FindAsync(id);
            if (locationAssets != null)
            {
                _context.LocationAssetsCollection.Remove(locationAssets);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationAssetsExists(Guid id)
        {
            return _context.LocationAssetsCollection.Any(e => e.Id == id);
        }
        
        private SelectList GetAssetSelectList(Guid? selectedValue = null)
        {
            var assets = _context.Assets.Include(a => a.LocationsAssetsCollection).ToList();
        
            assets = assets.Where(a => !a.LocationsAssetsCollection!.Any() || a.Id == selectedValue).ToList();
            return new SelectList(assets,
                nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
        }
    }
}