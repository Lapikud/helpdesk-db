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
    /// API controller for managing user roles.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class UserRolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly App.DTO.v1.Mappers.Identity.AppUserRoleMapper _mapper =
            new App.DTO.v1.Mappers.Identity.AppUserRoleMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public UserRolesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all AppUserRoles.
        /// </summary>
        /// <returns>List of AppUserRoles.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppUserRole>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppUserRole>>> GetUserRoles()
        {
            var data = await _context.UserRoles.ToListAsync();
            var res = data.Select(c => _mapper.Map(c)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a AppUserRole by id.
        /// </summary>
        /// <param name="id">The id of the AppUserRole.</param>
        /// <returns>The AppUserRole</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Identity.AppUserRole), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppUserRole>> GetAppUserRole(Guid id)
        {
            var appUserRole = await _context.UserRoles.FindAsync(id);

            if (appUserRole == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(appUserRole));;
        }

        /// <summary>
        /// Update a AppUserRole.
        /// </summary>
        /// <param name="id">The id of the AppUserRole to update.</param>
        /// <param name="appUserRole">The updated AppUserRole data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutAppUserRole(Guid id, AppUserRole appUserRole)
        {
            if (id != appUserRole.Id)
            {
                return BadRequest();
            }

            _context.Entry(_mapper.Map(appUserRole)!).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUserRoleExists(id))
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
        /// Create a appUserRole.
        /// </summary>
        /// <param name="appUserRole">The appUserRole to create.</param>
        /// <returns>The created appUserRole.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppRole>), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppUserRole>> PostAppUserRole(App.DTO.v1.CreateObjects.Identity.AppUserRoleCreate appUserRole)
        {
            var domainEntity = _mapper.Map(appUserRole);
            await _context.UserRoles.AddAsync(domainEntity);
            await _context.SaveChangesAsync();

            var dtoEntity = _mapper.Map(domainEntity);

            return CreatedAtAction("GetAppUserRole", new
            {
                id = domainEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoEntity);
        }

        /// <summary>
        /// Delete a appUserRole.
        /// </summary>
        /// <param name="id">The id of the appUserRole to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAppUserRole(Guid id)
        {
            var appUserRole = await _context.UserRoles.FindAsync(id);
            if (appUserRole == null)
            {
                return NotFound();
            }

            _context.UserRoles.Remove(appUserRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppUserRoleExists(Guid id)
        {
            return _context.UserRoles.Any(e => e.Id == id);
        }
    }
}
