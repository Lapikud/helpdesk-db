using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using App.BLL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class RoomsController : Controller
{
    private readonly IAppBLL _bll;

    public RoomsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: Rooms
    public async Task<IActionResult> Index()
    {
        var res = await _bll.RoomService.AllAsync();
        return View(res);
    }

    // GET: Rooms/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Rooms/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            // room.Id = Guid.NewGuid();
            await _bll.RoomService.AddAsync(room);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: Rooms/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _bll.RoomService.FindAsync(id.Value);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    // POST: Rooms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Room room)
    {
        if (id != room.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _bll.RoomService.UpdateAsync(room);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: Rooms/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _bll.RoomService.FindAsync(id.Value);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // POST: Rooms/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var cupboardInRooms = (await _bll.CupboardInRoomService.AllAsync()).Where(ca => ca.RoomId == id).ToList();
        foreach (var cir in cupboardInRooms)
        {
            await _bll.CupboardInRoomService.RemoveAsync(cir.Id);
        }
        await _bll.RoomService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}