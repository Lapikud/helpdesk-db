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
    /// API controller for managing owner assets.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins,members,pixels")]
    public class OwnerAssetsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.OwnerAssetsMapper _mapper =
            new App.DTO.v1.Mappers.OwnerAssetsMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public OwnerAssetsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all owner assets.
        /// </summary>
        /// <returns>List of owner assets.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.OwnerAssets>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<OwnerAssets>>> GetOwnerAssets()
        {
            var data = await _bll.OwnerAssetsService.AllAsync();
            var res = data.Select(oa => _mapper.Map(oa)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get an owner asset by id.
        /// </summary>
        /// <param name="id">The id of the owner asset.</param>
        /// <returns>The owner asset.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.OwnerAssets), StatusCodes.Status200OK)]
        public async Task<ActionResult<OwnerAssets>> GetOwnerAssets(Guid id)
        {
            var ownerAssets = await _bll.OwnerAssetsService.FindAsync(id);

            if (ownerAssets == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(ownerAssets)!);
        }

        /// <summary>
        /// Update an owner asset.
        /// </summary>
        /// <param name="id">The id of the owner asset to update.</param>
        /// <param name="ownerAssets">The updated owner asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutOwnerAssets(Guid id, OwnerAssets ownerAssets)
        {
            if (id != ownerAssets.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.OwnerAssetsService.UpdateAsync(_mapper.Map(ownerAssets)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.OwnerAssetsService.ExistsAsync(id))
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
        /// Create an owner asset.
        /// </summary>
        /// <param name="ownerAssets">The owner asset to create.</param>
        /// <returns>The created owner asset.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.OwnerAssets), StatusCodes.Status201Created)]
        public async Task<ActionResult<OwnerAssets>> PostOwnerAssets(App.DTO.v1.CreateObjects.OwnerAssetsCreate ownerAssets)
        {
            var bllEntity = _mapper.Map(ownerAssets);
            await _bll.OwnerAssetsService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoOwnerAsset = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetOwnerAssets", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoOwnerAsset);
        }

        /// <summary>
        /// Delete an owner asset.
        /// </summary>
        /// <param name="id">The id of the owner asset to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOwnerAssets(Guid id)
        {
            await _bll.OwnerAssetsService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
