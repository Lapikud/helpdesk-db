using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing locations.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LocationsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.LocationMapper _mapper =
            new App.DTO.v1.Mappers.LocationMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business logic.</param>
        public LocationsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all locations.
        /// </summary>
        /// <returns>List of locations.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Location>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.Location>>> GetLocations()
        {
            var data = await _bll.LocationService.AllAsync();
            var res = data.Select(l => _mapper.Map(l)!).OrderBy(l => l.LocationName).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a location by id.
        /// </summary>
        /// <param name="id">The id of the location.</param>
        /// <returns>The location.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Location), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Location>> GetLocation(Guid id)
        {
            var location = await _bll.LocationService.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }

        /// <summary>
        /// Update a location.
        /// </summary>
        /// <param name="id">The id of the location to update.</param>
        /// <param name="location">The updated location data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutLocation(Guid id, App.DTO.v1.Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.LocationService.UpdateAsync(_mapper.Map(location)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.LocationService.ExistsAsync(id))
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
        /// Create a location.
        /// </summary>
        /// <param name="location">The location to create.</param>
        /// <returns>The created location.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Location), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.Location>> PostLocation(App.DTO.v1.CreateObjects.LocationCreate location)
        {
            var bllEntity = _mapper.Map(location);
            await _bll.LocationService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoLocation = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetLocation", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoLocation);
        }

        /// <summary>
        /// Delete a location.
        /// </summary>
        /// <param name="id">The id of the location to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            var locationAssets = (await _bll.LocationAssetsService.AllAsync()).Where(la => la.LocationId == id).ToList();
            var locationsInCupboards = (await _bll.LocationInCupboardService.AllAsync()).Where(lic => lic.LocationId == id).ToList();
            foreach (var la in locationAssets)
            {
                await _bll.LocationAssetsService.RemoveAsync(la.Id);
            }
            foreach (var lic in locationsInCupboards)
            {
                await _bll.LocationInCupboardService.RemoveAsync(lic.Id);
            }
            await _bll.LocationService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
