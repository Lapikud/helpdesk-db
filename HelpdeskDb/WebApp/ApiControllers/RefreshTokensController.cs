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
    /// API controller for managing refresh tokens.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class RefreshTokensController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly App.DTO.v1.Mappers.Identity.RefreshTokensMapper _mapper =
            new App.DTO.v1.Mappers.Identity.RefreshTokensMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public RefreshTokensController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all AppRefreshTokens.
        /// </summary>
        /// <returns>List of AppRefreshTokens.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Identity.AppRefreshToken>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AppRefreshToken>>> GetAppRefreshTokens()
        {
            var data = await _context.RefreshTokens.ToListAsync();
            var res = data.Select(c => _mapper.Map(c)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get an AppRefreshToken by id.
        /// </summary>
        /// <param name="id">The id of the AppRefreshToken.</param>
        /// <returns>The AppRefreshToken</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Identity.AppRefreshToken), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppRefreshToken>> GetAppRefreshToken(Guid id)
        {
            var appRefreshToken = await _context.RefreshTokens.FindAsync(id);

            if (appRefreshToken == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(appRefreshToken));
        }
    }
}
