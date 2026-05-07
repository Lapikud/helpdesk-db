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
    public class LocationsInCupboardsController : Controller
    {
        private readonly AppDbContext _context;

        public LocationsInCupboardsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LocationsInCupboards
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LocationsInCupboards.Include(l => l.Cupboard).Include(l => l.Location);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LocationsInCupboards/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationInCupboard = await _context.LocationsInCupboards
                .Include(l => l.Cupboard)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (locationInCupboard == null)
            {
                return NotFound();
            }

            return View(locationInCupboard);
        }

        // GET: LocationsInCupboards/Create
        public IActionResult Create()
        {
            ViewData["CupboardId"] = new SelectList(_context.Cupboards, "Id", "CodeName");
            ViewData["LocationId"] = GetLocationSelectList();
            return View();
        }

        // POST: LocationsInCupboards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocationInCupboard locationInCupboard)
        {
            if (!await _context.LocationsInCupboards.AnyAsync(lic =>
                    lic.LocationId.Equals(locationInCupboard.LocationId)) && ModelState.IsValid)
            {
                locationInCupboard.Id = Guid.NewGuid();
                _context.Add(locationInCupboard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CupboardId"] =
                new SelectList(_context.Cupboards, "Id", "CodeName", locationInCupboard.CupboardId);
            ViewData["LocationId"] =
                GetLocationSelectList(locationInCupboard.LocationId);
            return View(locationInCupboard);
        }

        // GET: LocationsInCupboards/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationInCupboard = await _context.LocationsInCupboards.FindAsync(id);
            if (locationInCupboard == null)
            {
                return NotFound();
            }

            ViewData["CupboardId"] =
                new SelectList(_context.Cupboards, "Id", "CodeName", locationInCupboard.CupboardId);
            ViewData["LocationId"] =
                GetLocationSelectList(locationInCupboard.LocationId);
            return View(locationInCupboard);
        }

        // POST: LocationsInCupboards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, LocationInCupboard locationInCupboard)
        {
            if (id != locationInCupboard.Id)
            {
                return NotFound();
            }

            var changing = await _context.LocationsInCupboards.FirstAsync(lic => lic.Id.Equals(locationInCupboard.Id));
            var check = locationInCupboard.LocationId == changing.LocationId ||
                        await _context.LocationsInCupboards
                            .FirstOrDefaultAsync(lic => lic.LocationId.Equals(locationInCupboard.LocationId)) == null;
            
            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(locationInCupboard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationInCupboardExists(locationInCupboard.Id))
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

            ViewData["CupboardId"] =
                new SelectList(_context.Cupboards, "Id", "CodeName", locationInCupboard.CupboardId);
            ViewData["LocationId"] = GetLocationSelectList(locationInCupboard.LocationId);
            return View(locationInCupboard);
        }

        // GET: LocationsInCupboards/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var locationInCupboard = await _context.LocationsInCupboards
                .Include(l => l.Cupboard)
                .Include(l => l.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var locationInCupboard = await _context.LocationsInCupboards.FindAsync(id);
            if (locationInCupboard != null)
            {
                _context.LocationsInCupboards.Remove(locationInCupboard);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationInCupboardExists(Guid id)
        {
            return _context.LocationsInCupboards.Any(e => e.Id == id);
        }
        
        private SelectList GetLocationSelectList(Guid? selectedValue = null)
        {
            var locs = _context.Locations.Include(l => l.LocationsInCupboards).ToList();
        
            locs = locs.Where(l => !l.LocationsInCupboards!.Any() || l.Id == selectedValue).ToList();
        
            return new SelectList(locs,
                nameof(Location.Id), nameof(Location.LocationName), selectedValue);
        }
    }
}