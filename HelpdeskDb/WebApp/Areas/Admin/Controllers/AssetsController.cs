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
    public class AssetsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAppBLL _bll;

        public AssetsController(AppDbContext context, IAppBLL bll)
        {
            _context = context;
            _bll = bll;
        }

        // GET: Assets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Assets.ToListAsync());
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Assets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (ModelState.IsValid)
            {
                asset.Id = Guid.NewGuid();
                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Asset asset)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id))
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

            return View(asset);
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var locationAssets = (await _bll.LocationAssetsService.AllAsync()).Where(la => la.AssetId == id).ToList();
            var categoryAssets = (await _bll.CategoryAssetsService.AllAsync()).Where(ca => ca.AssetId == id).ToList();
            var ownerAssets = (await _bll.OwnerAssetsService.AllAsync()).Where(oa => oa.AssetId == id).ToList();
            var removedAssets = (await _bll.RemovedAssetsService.AllAsync()).Where(ra => ra.AssetId == id).ToList();
            var assetReservations = (await _bll.AssetReservationService.AllAsync()).Where(ar => ar.AssetId == id).ToList();
            foreach (var la in locationAssets)
            {
                await _bll.LocationAssetsService.RemoveAsync(la.Id);
            }
            foreach (var ca in categoryAssets)
            {
                await _bll.CategoryAssetsService.RemoveAsync(ca.Id);
            }
            foreach (var oa in ownerAssets)
            {
                await _bll.OwnerAssetsService.RemoveAsync(oa.Id);
            }
            foreach (var ra in removedAssets)
            {
                await _bll.RemovedAssetsService.RemoveAsync(ra.Id);
            }
            foreach (var ar in assetReservations)
            {
                await _bll.AssetReservationService.RemoveAsync(ar.Id, ar.UserId);
            }
            await _bll.AssetService.RemoveAsync(id);
        
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssetExists(Guid id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }
    }
}