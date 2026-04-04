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
    /// API controller for managing users.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly App.DTO.v1.Mappers.Identity.AppUserMapper _mapper =
            new App.DTO.v1.Mappers.Identity.AppUserMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>List of users.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppUser>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var data = await _context.Users.ToListAsync();
            var res = data.Select(c => _mapper.Map(c)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a AppUser by id.
        /// </summary>
        /// <param name="id">The id of the AppUser.</param>
        /// <returns>The AppUser</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Identity.AppUser), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppUser>> GetAppUser(Guid id)
        {
            var appUser = await _context.Users.FindAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(appUser));
        }

        private bool AppUserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
