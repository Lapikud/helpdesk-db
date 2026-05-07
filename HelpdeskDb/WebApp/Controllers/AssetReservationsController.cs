using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.BLL.DTO.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Base.Helpers;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class AssetReservationsController : Controller
    {
        private readonly IAppBLL _bll;

        public AssetReservationsController(IAppBLL bll)
        {
            _bll = bll;
        }

        // GET: AssetReservations
        public async Task<IActionResult> Index()
        {
            var res = await _bll.AssetReservationService.AllAsync();
            return View(res);
        }
        

        // GET: AssetReservations/Create
        public async Task<IActionResult> Create()
        {
            var vm = new AssetReservationCreateEditVm()
            {
                AssetSelectList = await GetAssetSelectListAsync(),
                UserSelectList = GetUserSelectList(),
            };
            return View(vm);
        }

        // POST: AssetReservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetReservationCreateEditVm vm)
        {
            if (await _bll.AssetReservationService.HasActiveOrFutureReservation(vm.AssetReservation.AssetId, User.GetUserId()))
            {
                ModelState.AddModelError(nameof(AssetReservation.User),
                    "You already have an active reservation for this asset.");
            }
            
            var reservationFrom = vm.AssetReservation.ReservationFrom.ToUniversalTime();
            var reservationTo = vm.AssetReservation.ReservationTo.ToUniversalTime();
            
            if (!await _bll.AssetReservationService.IsAssetReservationAvailable(vm.AssetReservation.AssetId,
                    reservationFrom, reservationTo))
            {
                ModelState.AddModelError(nameof(AssetReservation.ReservationFrom),
                    "Reservation for that time is unavailable.");
                ModelState.AddModelError(nameof(AssetReservation.ReservationTo),
                    "Reservation for that time is unavailable.");
            }

            if (ModelState.IsValid && await _bll.AssetReservationService.IsAssetReservationAvailable(
                    vm.AssetReservation.AssetId,
                    reservationFrom, reservationTo))
            {
                await _bll.AssetReservationService.UserReserveAsset(vm.AssetReservation.UserId,
                    vm.AssetReservation.AssetId, reservationFrom,
                    reservationTo);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.AssetSelectList = await GetAssetSelectListAsync(vm.AssetReservation.AssetId);
            vm.UserSelectList = GetUserSelectList(vm.AssetReservation.UserId);
            return View(vm);
        }

        // GET: AssetReservations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assetReservation = await _bll.AssetReservationService.FindAsync(id.Value, User.GetUserId());
            if (assetReservation == null)
            {
                return NotFound();
            }
            
            assetReservation.ReservationFrom = assetReservation.ReservationFrom.ToLocalTime();
            assetReservation.ReservationTo = assetReservation.ReservationTo.ToLocalTime();

            var vm = new AssetReservationCreateEditVm()
            {
                AssetSelectList = await GetAssetSelectListAsync(),
                UserSelectList = GetUserSelectList(),
                AssetReservation = assetReservation
            };
            return View(vm);
        }

        // POST: AssetReservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AssetReservationCreateEditVm vm)
        {
            if (id != vm.AssetReservation.Id)
            {
                return NotFound();
            }

            var changing = await _bll.AssetReservationService.FindAsync(vm.AssetReservation.Id, User.GetUserId());

            var check = vm.AssetReservation.AssetId == changing!.AssetId ||
                        await _bll.AssetReservationService.GetAssetReservationsByUserIdAndAssetId(
                            vm.AssetReservation.UserId, vm.AssetReservation.AssetId) == null;

            if (ModelState.IsValid && check)
            {
                var reservationFrom = vm.AssetReservation.ReservationFrom.ToUniversalTime();
                var reservationTo = vm.AssetReservation.ReservationTo.ToUniversalTime();

                vm.AssetReservation.ReservationFrom = reservationFrom;
                vm.AssetReservation.ReservationTo = reservationTo;
                
                await _bll.AssetReservationService.UpdateAsync(vm.AssetReservation);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            vm.AssetSelectList = await GetAssetSelectListAsync(vm.AssetReservation.AssetId);
            vm.UserSelectList = GetUserSelectList(vm.AssetReservation.UserId);
            return View(vm);
        }

        // GET: AssetReservations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assetReservation = await _bll.AssetReservationService.FindAsync(id.Value, User.GetUserId());
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
            await _bll.AssetReservationService.RemoveAsync(id, User.GetUserId());
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private async Task<SelectList> GetAssetSelectListAsync(Guid? selectedValue = null)
        {
            var assets = await _bll.AssetService.AllAsync();

            return new SelectList(assets,
                nameof(Asset.Id), nameof(Asset.AssetName), selectedValue);
        }

        private SelectList GetUserSelectList(Guid? selectedValue = null)
        {
            return new SelectList(
                new[]
                {
                    new
                    {
                        Id = User.GetUserId(),
                        Username = User.Identity?.Name
                    }
                },
                nameof(AppUser.Id), nameof(AppUser.Username), selectedValue);
        }
    }
}