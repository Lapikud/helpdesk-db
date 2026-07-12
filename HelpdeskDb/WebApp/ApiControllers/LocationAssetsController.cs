using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.DTO.v1;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing location assets.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins,members,pixels")]
    public class LocationAssetsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.LocationAssetsMapper _mapper =
            new App.DTO.v1.Mappers.LocationAssetsMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public LocationAssetsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all location assets.
        /// </summary>
        /// <returns>List of location assets.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.LocationAssets>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LocationAssets>>> GetLocationAssetsCollection()
        {
            var data = await _bll.LocationAssetsService.AllAsync();
            var res = data.Select(la => _mapper.Map(la)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a location asset by id.
        /// </summary>
        /// <param name="id">The id of the location asset.</param>
        /// <returns>The category.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.LocationAssets), StatusCodes.Status200OK)]
        public async Task<ActionResult<LocationAssets>> GetLocationAssets(Guid id)
        {
            var locationAsset = await _bll.LocationAssetsService.FindAsync(id);

            if (locationAsset == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(locationAsset)!);
        }

        /// <summary>
        /// Update a location assets.
        /// </summary>
        /// <param name="id">The id of the location asset to update.</param>
        /// <param name="locationAssets">The updated location asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutLocationAssets(Guid id, LocationAssets locationAssets)
        {
            if (id != locationAssets.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.LocationAssetsService.UpdateAsync(_mapper.Map(locationAssets)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.LocationAssetsService.ExistsAsync(id))
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
        /// Create a location asset.
        /// </summary>
        /// <param name="locationAssets">The location asset to create.</param>
        /// <returns>The created location asset.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.LocationAssets), StatusCodes.Status201Created)]
        public async Task<ActionResult<LocationAssets>> PostLocationAssets(App.DTO.v1.CreateObjects.LocationAssetsCreate locationAssets)
        {
            var bllEntity = _mapper.Map(locationAssets);
            await _bll.LocationAssetsService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoLocationAsset = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetLocationAssets", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoLocationAsset);
        }

        /// <summary>
        /// Delete a location asset.
        /// </summary>
        /// <param name="id">The id of the location asset to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteLocationAssets(Guid id)
        {
            await _bll.LocationAssetsService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
