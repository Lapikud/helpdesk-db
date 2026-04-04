using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
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
    public class LocationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAppBLL _bll;

        public LocationsController(AppDbContext context, IAppBLL bll)
        {
            _context = context;
            _bll = bll;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Locations.OrderBy(l => l.LocationName).ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
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
        public async Task<IActionResult> Create(Location location)
        {
            if (ModelState.IsValid)
            {
                location.Id = Guid.NewGuid();
                _context.Add(location);
                await _context.SaveChangesAsync();
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

            var location = await _context.Locations.FindAsync(id);
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
        public async Task<IActionResult> Edit(Guid id, Location location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
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
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
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

        private bool LocationExists(Guid id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
