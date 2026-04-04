using System.Text;
using System.Text.Encodings.Web;
using App.DAL.EF;
using App.DTO.v1.Identity;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing user roles.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class UserManagementApiController : ControllerBase
    {
        private readonly ILogger<UserManagementApiController> _logger;
        private readonly AppDbContext _context;
        
        public UserManagementApiController(
            ILogger<UserManagementApiController> logger,
            AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await _context.Users.OrderBy(u => u.Username).ToListAsync();
            return Ok(users);
        }
        
        // POST: api/UserManagementApi/RoleAdd
        [HttpPost("RoleAdd")]
        public async Task<IActionResult> AddRoleToUser(Guid userId, Guid roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found.");

            var alreadyHasRole = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (alreadyHasRole) return BadRequest("User already has this role.");

            var newUserRole = new App.Domain.Identity.AppUserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId
            };

            await _context.UserRoles.AddAsync(newUserRole);
            await _context.SaveChangesAsync();

            return Ok("Role added.");
        }
        
        // DELETE: api/UserManagementApi/RoleRemove
        [HttpDelete("RoleRemove")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound("User not found.");

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null) return BadRequest("User does not have this role.");

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return Ok("Role removed.");
        }
    }
}

