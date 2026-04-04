using System.Text;
using System.Text.Encodings.Web;
using App.DAL.EF;
using App.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "admins,helpdesk_db_admins")]
public class UserManagementController : Controller
{
    private readonly ILogger<UserManagementController> _logger;
    private readonly AppDbContext _context;

    public UserManagementController(ILogger<UserManagementController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();

        var allRoles = await _context.Roles.ToListAsync();

        var model = users.Select(u => new UserManagementVm()
        {
            User = u,
            AllRoles = allRoles,
            UserRoles = u.UserRoles!.Select(ur => ur.Role!).ToList()
        }).ToList();
        
        return View(model);
    }
    
    
    // TODO: If this view is staying, then need to update claims on each role add/remove

    public async Task<IActionResult> RoleRemove(Guid userId, Guid roleId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return RedirectToAction("Index", new { error = "User Not Found" });
        }
        
        if (!await _context.UserRoles.AnyAsync(ur => 
                ur.UserId.Equals(userId) &&
                ur.RoleId.Equals(roleId))
           )
        {
            return RedirectToAction("Index", new { error = "User don't have this role" });
        }

        var userRoleToRemove = await _context.UserRoles.FirstAsync(ur => ur.UserId.Equals(userId) && ur.RoleId.Equals(roleId));
        _context.UserRoles.Remove(userRoleToRemove);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");

        // var result = await _userManager.RemoveFromRoleAsync(user, role);
        // if (result.Succeeded)
        // {
        //     return RedirectToAction("Index");
        // }

        // return RedirectToAction("Index", new { error = result.Errors.Select(e => e.Description).First() });
    }

    public async Task<IActionResult> RoleAdd(Guid userId, Guid roleId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return RedirectToAction("Index", new { error = "User Not Found" });
        }

        if (await _context.UserRoles.AnyAsync(ur => 
                ur.UserId.Equals(userId) &&
                ur.RoleId.Equals(roleId))
            )
        {
            return RedirectToAction("Index", new { error = "User already has this role" });
        }

        var newUserRole = new AppUserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId
        };

        await _context.UserRoles.AddAsync(newUserRole);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");

        // var result = await _userManager.AddToRoleAsync(user, role);
        // if (result.Succeeded)
        // {
        //     var addedRole = await _context.Roles.FirstAsync(r => r.Name!.Equals(role));
        //     var appUserRole =
        //         await _context.UserRoles.FirstAsync(c => c.UserId.Equals(user.Id) && c.RoleId.Equals(addedRole.Id));
        //     Console.WriteLine(
        //         $"appUserRole userId: {appUserRole.UserId}, roleId: {appUserRole.RoleId}, id: {appUserRole.Id}");
        //     appUserRole.Id = Guid.NewGuid();
        //     Console.WriteLine(
        //         $"appUserRole userId: {appUserRole.UserId}, roleId: {appUserRole.RoleId}, id: {appUserRole.Id}");
        //     _context.Entry(appUserRole).Property(ur => ur.Id).IsModified = true;
        //     await _context.SaveChangesAsync();
        //     return RedirectToAction("Index");
        // }
        //
        //
        // return RedirectToAction("Index", new { error = result.Errors.Select(e => e.Description).First() });
    }
}