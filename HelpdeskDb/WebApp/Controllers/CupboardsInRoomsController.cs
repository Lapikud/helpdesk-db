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
using App.DAL.EF;
using Microsoft.AspNetCore.Authorization;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = "admins,helpdesk_db_admins")]
public class CupboardsInRoomsController : Controller
{
    private readonly IAppBLL _bll;

    public CupboardsInRoomsController(IAppBLL bll)
    {
        _bll = bll;
    }

    // GET: CupboardsInRooms
    public async Task<IActionResult> Index()
    {
        var res = await _bll.CupboardInRoomService.AllAsync();
        return View(res);
    }
    

    // GET: CupboardsInRooms/Create
    public async Task<IActionResult> Create()
    {
        var vm = new CupboardInRoomCreateEditViewModel()
        {
            CupboardSelectList = await GetCupboardSelectListAsync(),
            RoomSelectList = await GetRoomSelectListAsync(),
        };
        return View(vm);
    }

    // POST: CupboardsInRooms/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CupboardInRoomCreateEditViewModel vm)
    {
        if (await _bll.CupboardInRoomService
                .GetCupboardInRoomByCupboardId(vm.CupboardInRoom.CupboardId) == null &&
            ModelState.IsValid)
        {
            // cupboardInRoom.Id = Guid.NewGuid();
            await _bll.CupboardInRoomService.AddAsync(vm.CupboardInRoom);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.CupboardSelectList = await GetCupboardSelectListAsync(vm.CupboardInRoom.CupboardId);
        vm.RoomSelectList = await GetRoomSelectListAsync(vm.CupboardInRoom.RoomId);
        return View(vm);
    }

    // GET: CupboardsInRooms/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cupboardInRoom = await _bll.CupboardInRoomService.FindAsync(id.Value);
        if (cupboardInRoom == null)
        {
            return NotFound();
        }

        var vm = new CupboardInRoomCreateEditViewModel()
        {
            CupboardSelectList = await GetCupboardSelectListAsync(cupboardInRoom.CupboardId),
            RoomSelectList = await GetRoomSelectListAsync(),
            CupboardInRoom = cupboardInRoom,
        };
        return View(vm);
    }

    // POST: CupboardsInRooms/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CupboardInRoomCreateEditViewModel vm)
    {
        if (id != vm.CupboardInRoom.Id)
        {
            return NotFound();
        }

        var changing = await _bll.CupboardInRoomService.FindAsync(vm.CupboardInRoom.Id);
        var check = vm.CupboardInRoom.CupboardId == changing!.CupboardId ||
                    await _bll.CupboardInRoomService
                        .GetCupboardInRoomByCupboardId(vm.CupboardInRoom.CupboardId) == null;

        if (ModelState.IsValid && check)
        {
            await _bll.CupboardInRoomService.UpdateAsync(vm.CupboardInRoom);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        vm.CupboardSelectList = await GetCupboardSelectListAsync(vm.CupboardInRoom.CupboardId);
        vm.RoomSelectList = await GetRoomSelectListAsync(vm.CupboardInRoom.RoomId);
        return View(vm);
    }

    // GET: CupboardsInRooms/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cupboardInRoom = await _bll.CupboardInRoomService.FindAsync(id.Value);
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
        await _bll.CupboardInRoomService.RemoveAsync(id);
        await _bll.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> GetCupboardSelectListAsync(Guid? selectedValue = null)
    {
        var cupboards = await _bll.CupboardService.AllAsync();
        
        cupboards = cupboards.Where(c => !c.CupboardsInRooms!.Any() || c.Id == selectedValue).ToList();
        return new SelectList(cupboards,
            nameof(Cupboard.Id), nameof(Cupboard.CodeName), selectedValue);
    }

    private async Task<SelectList> GetRoomSelectListAsync(Guid? selectedValue = null)
    {
        return new SelectList(await _bll.RoomService.AllAsync(),
            nameof(Room.Id), nameof(Room.RoomName), selectedValue);
    }
}