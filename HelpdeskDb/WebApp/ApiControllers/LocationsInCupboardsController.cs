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
    /// API controller for managing locations in cupboards.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class LocationsInCupboardsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.LocationInCupboardMapper _mapper =
            new App.DTO.v1.Mappers.LocationInCupboardMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business logic.</param>
        public LocationsInCupboardsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all locations in cupboards.
        /// </summary>
        /// <returns>List of locations in cupboards.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.LocationInCupboard>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.LocationInCupboard>>> GetLocationsInCupboards()
        {
            var data = await _bll.LocationInCupboardService.AllAsync();
            var res = data.Select(lic => _mapper.Map(lic)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a location in a cupboard by id.
        /// </summary>
        /// <param name="id">The id of the location in the cupboard.</param>
        /// <returns>The location in the cupboard.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.LocationInCupboard), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.LocationInCupboard>> GetLocationInCupboard(Guid id)
        {
            var locationInCupboard = await _bll.LocationInCupboardService.FindAsync(id);

            if (locationInCupboard == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(locationInCupboard)!);
        }

        /// <summary>
        /// Update a location in a cupboard.
        /// </summary>
        /// <param name="id">The id of the cupboard to update.</param>
        /// <param name="locationInCupboard">The updated location in the cupboard data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutLocationInCupboard(Guid id, App.DTO.v1.LocationInCupboard locationInCupboard)
        {
            if (id != locationInCupboard.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.LocationInCupboardService.UpdateAsync(_mapper.Map(locationInCupboard)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.LocationInCupboardService.ExistsAsync(id))
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
        /// Create a location in a cupboard.
        /// </summary>
        /// <param name="locationInCupboard">The location in the cupboard to create.</param>
        /// <returns>The created location in the cupboard.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.LocationInCupboard), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.LocationInCupboard>> PostLocationInCupboard(App.DTO.v1.CreateObjects.LocationInCupboardCreate locationInCupboard)
        {
            var bllEntity = _mapper.Map(locationInCupboard);
            await _bll.LocationInCupboardService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoLocationInCupboard = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetlocationInCupboard", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoLocationInCupboard);
        }

        /// <summary>
        /// Delete a location in a cupboard.
        /// </summary>
        /// <param name="id">The id of the location in the cupboard to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteLocationInCupboard(Guid id)
        {
            await _bll.LocationInCupboardService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
