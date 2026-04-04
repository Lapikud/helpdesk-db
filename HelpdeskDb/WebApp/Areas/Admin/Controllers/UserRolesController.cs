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
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admins,helpdesk_db_admins")]
    public class UserRolesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAppBLL _bll;

        public UserRolesController(AppDbContext context, IAppBLL bll)
        {
            _context = context;
            _bll = bll;
        }

        // GET: UserRoles
        public async Task<IActionResult> Index()
        {
            // var appDbContext = _context.UserRoles.Include(a => a.Role).Include(a => a.User);
            // return View(await appDbContext.ToListAsync());
            var res = await _bll.AppUserRoleService.AllAsync();
            return View(res);
        }

        // GET: UserRoles/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUserRole = await _bll.AppUserRoleService.FindAsync(id.Value);
            if (appUserRole == null)
            {
                return NotFound();
            }

            return View(appUserRole);
            
        }

        // GET: UserRoles/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: UserRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserRole appUserRole)
        {
            if (ModelState.IsValid)
            {
                await _bll.AppUserRoleService.AddAsync(appUserRole);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // if (ModelState.IsValid)
            // {
            //     Console.WriteLine($"user id: {appUserRole.UserId}, role id: {appUserRole.RoleId}");
            //     appUserRole.Id = Guid.NewGuid();
            //     _context.Add(appUserRole);
            //     await _context.SaveChangesAsync();
            //     return RedirectToAction(nameof(Index));
            // }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", appUserRole.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", appUserRole.UserId);
            return View(appUserRole);
        }

        // GET: UserRoles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUserRole = await _bll.AppUserRoleService.FindAsync(id.Value);
            if (appUserRole == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", appUserRole.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", appUserRole.UserId);
            return View(appUserRole);
        }

        // POST: UserRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, App.BLL.DTO.Identity.AppUserRole appUserRole)
        {
            if (id != appUserRole.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Console.WriteLine($"userRoleId: {appUserRole.Id}, userId: {appUserRole.UserId}, roleId: {appUserRole.RoleId}");
                await _bll.AppUserRoleService.UpdateAsync(appUserRole);
                await _bll.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", appUserRole.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", appUserRole.UserId);
            return View(appUserRole);
        }

        // GET: UserRoles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUserRole = await _bll.AppUserRoleService.FindAsync(id.Value);
            if (appUserRole == null)
            {
                return NotFound();
            }

            return View(appUserRole);
        }

        // POST: UserRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _bll.AppUserRoleService.RemoveAsync(id);
            await _bll.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
