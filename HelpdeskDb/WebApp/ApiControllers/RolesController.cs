using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.DTO.v1.Identity;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing roles.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly App.DTO.v1.Mappers.Identity.AppRoleMapper _mapper =
            new App.DTO.v1.Mappers.Identity.AppRoleMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all roles.
        /// </summary>
        /// <returns>List of roles.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppRole>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppRole>>> GetRoles()
        {
            var data = await _context.Roles.ToListAsync();
            var res = data.Select(c => _mapper.Map(c)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a role by id.
        /// </summary>
        /// <param name="id">The id of the role.</param>
        /// <returns>The role</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Identity.AppRole), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppRole>> GetAppRole(Guid id)
        {
            var appRole = await _context.Roles.FindAsync(id);

            if (appRole == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(appRole));
        }

        /// <summary>
        /// Update a role.
        /// </summary>
        /// <param name="id">The id of the role to update.</param>
        /// <param name="appRole">The updated role data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutAppRole(Guid id, AppRole appRole)
        {
            if (id != appRole.Id)
            {
                return BadRequest();
            }

            _context.Entry(_mapper.Map(appRole)!).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppRoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Create a role.
        /// </summary>
        /// <param name="appRole">The role to create.</param>
        /// <returns>The created role.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppRole>), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppRole>> PostAppRole(App.DTO.v1.CreateObjects.Identity.AppRoleCreate appRole)
        {
            var domainEntity = _mapper.Map(appRole);
            await _context.Roles.AddAsync(domainEntity);
            await _context.SaveChangesAsync();

            var dtoEntity = _mapper.Map(domainEntity);

            return CreatedAtAction("GetAppRole", new
            {
                id = domainEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoEntity);
        }

        /// <summary>
        /// Delete a role.
        /// </summary>
        /// <param name="id">The id of the role to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAppRole(Guid id)
        {
            var appRole = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (appRole == null)
            {
                return NotFound();
            }
            
            if (appRole.UserRoles != null)
            {
                _context.UserRoles.RemoveRange(appRole.UserRoles);
            }
            _context.Roles.Remove(appRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppRoleExists(Guid id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}