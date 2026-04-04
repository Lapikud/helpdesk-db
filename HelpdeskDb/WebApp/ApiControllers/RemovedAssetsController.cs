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
    /// API controller for managing removed assets.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RemovedAssetsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.RemovedAssetsMapper _mapper =
            new App.DTO.v1.Mappers.RemovedAssetsMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public RemovedAssetsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all removed assets.
        /// </summary>
        /// <returns>List of removed assets.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.RemovedAssets>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.RemovedAssets>>> GetRemovedAssetsCollection()
        {
            var data = await _bll.RemovedAssetsService.AllAsync();
            var res = data.Select(ra => _mapper.Map(ra)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a removed asset by id.
        /// </summary>
        /// <param name="id">The id of the removed asset.</param>
        /// <returns>The removed asset.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.RemovedAssets), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.RemovedAssets>> GetRemovedAssets(Guid id)
        {
            var removedAssets = await _bll.RemovedAssetsService.FindAsync(id);

            if (removedAssets == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(removedAssets)!);
        }

        /// <summary>
        /// Update a removed asset.
        /// </summary>
        /// <param name="id">The id of the removed asset to update.</param>
        /// <param name="removedAssets">The updated removed asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutRemovedAssets(Guid id, App.DTO.v1.RemovedAssets removedAssets)
        {
            if (id != removedAssets.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.RemovedAssetsService.UpdateAsync(_mapper.Map(removedAssets)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.RemovedAssetsService.ExistsAsync(id))
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
        /// Create a removed asset.
        /// </summary>
        /// <param name="removedAssets">The removed asset to create.</param>
        /// <returns>The created removed asset.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.RemovedAssets), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.RemovedAssets>> PostRemovedAssets(App.DTO.v1.CreateObjects.RemovedAssetsCreate removedAssets)
        {
            var bllEntity = _mapper.Map(removedAssets);
            await _bll.RemovedAssetsService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoRemovedAsset = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetRemovedAssets", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoRemovedAsset);
        }

        /// <summary>
        /// Delete a removed asset.
        /// </summary>
        /// <param name="id">The id of the removed asset to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteRemovedAssets(Guid id)
        {
            await _bll.RemovedAssetsService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
