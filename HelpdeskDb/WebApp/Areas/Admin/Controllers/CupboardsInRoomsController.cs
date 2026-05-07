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
    public class CupboardsInRoomsController : Controller
    {
        private readonly AppDbContext _context;

        public CupboardsInRoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CupboardsInRooms
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.CupboardsInRooms.Include(c => c.Cupboard).Include(c => c.Room);
            return View(await appDbContext.ToListAsync());
        }

        // GET: CupboardsInRooms/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupboardInRoom = await _context.CupboardsInRooms
                .Include(c => c.Cupboard)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cupboardInRoom == null)
            {
                return NotFound();
            }

            return View(cupboardInRoom);
        }

        // GET: CupboardsInRooms/Create
        public IActionResult Create()
        {
            ViewData["CupboardId"] = GetCupboardSelectList();
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName");
            return View();
        }

        // POST: CupboardsInRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CupboardInRoom cupboardInRoom)
        {
            if (!await _context.CupboardsInRooms.AnyAsync(cir => cir.CupboardId.Equals(cupboardInRoom.CupboardId)) &&
                ModelState.IsValid)
            {
                cupboardInRoom.Id = Guid.NewGuid();
                _context.Add(cupboardInRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CupboardId"] = GetCupboardSelectList(cupboardInRoom.CupboardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", cupboardInRoom.RoomId);
            return View(cupboardInRoom);
        }

        // GET: CupboardsInRooms/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupboardInRoom = await _context.CupboardsInRooms.FindAsync(id);
            if (cupboardInRoom == null)
            {
                return NotFound();
            }

            ViewData["CupboardId"] = GetCupboardSelectList(cupboardInRoom.CupboardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", cupboardInRoom.RoomId);
            return View(cupboardInRoom);
        }

        // POST: CupboardsInRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CupboardInRoom cupboardInRoom)
        {
            if (id != cupboardInRoom.Id)
            {
                return NotFound();
            }

            var changing = await _context.CupboardsInRooms.FirstAsync(cir => cir.Id.Equals(cupboardInRoom.Id));
            var check = cupboardInRoom.CupboardId == changing.CupboardId ||
                        await _context.CupboardsInRooms.FirstOrDefaultAsync(cir =>
                            cir.CupboardId.Equals(cupboardInRoom.CupboardId)) == null;

            if (ModelState.IsValid && check)
            {
                try
                {
                    _context.Update(cupboardInRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CupboardInRoomExists(cupboardInRoom.Id))
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

            ViewData["CupboardId"] = GetCupboardSelectList(cupboardInRoom.CupboardId);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomName", cupboardInRoom.RoomId);
            return View(cupboardInRoom);
        }

        // GET: CupboardsInRooms/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cupboardInRoom = await _context.CupboardsInRooms
                .Include(c => c.Cupboard)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cupboardInRoom == null)
            {
                return NotFound();
            }

            return View(cupboardInRoom);
        }

        // POST: CupboardsInRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cupboardInRoom = await _context.CupboardsInRooms.FindAsync(id);
            if (cupboardInRoom != null)
            {
                _context.CupboardsInRooms.Remove(cupboardInRoom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CupboardInRoomExists(Guid id)
        {
            return _context.CupboardsInRooms.Any(e => e.Id == id);
        }
        
        private SelectList GetCupboardSelectList(Guid? selectedValue = null)
        {
            var cupboards = _context.Cupboards.Include(a => a.CupboardsInRooms).ToList();
        
            cupboards = cupboards.Where(c => !c.CupboardsInRooms!.Any() || c.Id == selectedValue).ToList();
            return new SelectList(cupboards,
                nameof(Cupboard.Id), nameof(Cupboard.CodeName), selectedValue);
        }
    }
}