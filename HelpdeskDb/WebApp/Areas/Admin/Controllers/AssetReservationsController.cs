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
    public class AssetReservationsController : Controller
    {
        private readonly AppDbContext _context;

        public AssetReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AssetReservations
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.AssetReservations.Include(a => a.Asset).Include(a => a.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: AssetReservations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assetReservation = await _context.AssetReservations
                .Include(a => a.Asset)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assetReservation == null)
            {
                return NotFound();
            }

            return View(assetReservation);
        }

        // GET: AssetReservations/Create
        public IActionResult Create()
        {
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "AssetName");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: AssetReservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssetId,ReservationFrom,ReservationTo,UserId,Id,CreatedBy,CreatedAt,ChangedBy,ChangedAt,SysNotes")] AssetReservation assetReservation)
        {
            if (ModelState.IsValid)
            {
                assetReservation.Id = Guid.NewGuid();
                _context.Add(assetReservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "AssetName", assetReservation.AssetId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", assetReservation.UserId);
            return View(assetReservation);
        }

        // GET: AssetReservations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assetReservation = await _context.AssetReservations.FindAsync(id);
            if (assetReservation == null)
            {
                return NotFound();
            }
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "AssetName", assetReservation.AssetId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", assetReservation.UserId);
            return View(assetReservation);
        }

        // POST: AssetReservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AssetId,ReservationFrom,ReservationTo,UserId,Id,CreatedBy,CreatedAt,ChangedBy,ChangedAt,SysNotes")] AssetReservation assetReservation)
        {
            if (id != assetReservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assetReservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetReservationExists(assetReservation.Id))
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
            ViewData["AssetId"] = new SelectList(_context.Assets, "Id", "AssetName", assetReservation.AssetId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", assetReservation.UserId);
            return View(assetReservation);
        }

        // GET: AssetReservations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assetReservation = await _context.AssetReservations
                .Include(a => a.Asset)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assetReservation == null)
            {
                return NotFound();
            }

            return View(assetReservation);
        }

        // POST: AssetReservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var assetReservation = await _context.AssetReservations.FindAsync(id);
            if (assetReservation != null)
            {
                _context.AssetReservations.Remove(assetReservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssetReservationExists(Guid id)
        {
            return _context.AssetReservations.Any(e => e.Id == id);
        }
    }
}
