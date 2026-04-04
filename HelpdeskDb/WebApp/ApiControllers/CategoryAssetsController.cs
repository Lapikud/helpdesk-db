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
    /// API controller for managing category assets.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins,members,pixels")]
    public class CategoryAssetsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.CategoryAssetsMapper _mapper =
            new App.DTO.v1.Mappers.CategoryAssetsMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public CategoryAssetsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all category assets.
        /// </summary>
        /// <returns>List of category assets.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.CategoryAssets>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CategoryAssets>>> GetCategoryAssetsCollection()
        {
            var data = await _bll.CategoryAssetsService.AllAsync();
            var res = data.Select(ca => _mapper.Map(ca)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a category asset by id.
        /// </summary>
        /// <param name="id">The id of the category asset.</param>
        /// <returns>The category asset.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.CategoryAssets), StatusCodes.Status200OK)]
        public async Task<ActionResult<CategoryAssets>> GetCategoryAssets(Guid id)
        {
            var categoryAsset = await _bll.CategoryAssetsService.FindAsync(id);

            if (categoryAsset == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(categoryAsset)!);
        }

        /// <summary>
        /// Update a category asset.
        /// </summary>
        /// <param name="id">The id of the category asset to update.</param>
        /// <param name="categoryAssets">The updated category asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutCategoryAssets(Guid id, CategoryAssets categoryAssets)
        {
            if (id != categoryAssets.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.CategoryAssetsService.UpdateAsync(_mapper.Map(categoryAssets)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.CategoryAssetsService.ExistsAsync(id))
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
        /// Create a category asset.
        /// </summary>
        /// <param name="categoryAssets">The category asset to create.</param>
        /// <returns>The created category asset.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.CategoryAssets), StatusCodes.Status201Created)]
        public async Task<ActionResult<CategoryAssets>> PostCategoryAssets(App.DTO.v1.CreateObjects.CategoryAssetsCreate categoryAssets)
        {
            var bllEntity = _mapper.Map(categoryAssets);
            await _bll.CategoryAssetsService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoCategoryAsset = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetCategoryAssets", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoCategoryAsset);
        }

        /// <summary>
        /// Delete a category asset.
        /// </summary>
        /// <param name="id">The id of the category asset to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCategoryAssets(Guid id)
        {
            await _bll.CategoryAssetsService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
