using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.BLL.Contracts;
using App.DAL.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.DTO.v1;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing categories.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CategoriesController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.CategoryMapper _mapper =
            new App.DTO.v1.Mappers.CategoryMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public CategoriesController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Category>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.Category>>> GetCategories()
        {
            var data = await _bll.CategoryService.AllAsync();
            var res = data.Select(c => _mapper.Map(c)!).OrderBy(c => c.CategoryName).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a category by id.
        /// </summary>
        /// <param name="id">The id of the category.</param>
        /// <returns>The category.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Category), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Category>> GetCategory(Guid id)
        {
            var category = await _bll.CategoryService.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(category)!);
        }

        /// <summary>
        /// Update a category.
        /// </summary>
        /// <param name="id">The id of the category to update.</param>
        /// <param name="category">The updated category data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutCategory(Guid id, App.DTO.v1.Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }
            
            var categories = await _bll.CategoryService.AllAsync();
            if (categories.Any(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new Message(App.Resources.Errors.ValidationErrors.CategoryNameExists));
            }

            try
            {
                await _bll.CategoryService.UpdateAsync(_mapper.Map(category)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.CategoryService.ExistsAsync(id))
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
        /// Create a category.
        /// </summary>
        /// <param name="category">The category to create.</param>
        /// <returns>The created category.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Category), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<App.DTO.v1.Category>> PostCategory(App.DTO.v1.CreateObjects.CategoryCreate category)
        {
            var categories = await _bll.CategoryService.AllAsync();
            if (categories.Any(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new Message(App.Resources.Errors.ValidationErrors.CategoryNameExists));
            }
            
            var bllEntity = _mapper.Map(category);
            await _bll.CategoryService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoCategory = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetCategory", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoCategory);
        }

        /// <summary>
        /// Delete a category.
        /// </summary>
        /// <param name="id">The id of the category to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var categoryAssets = (await _bll.CategoryAssetsService.AllAsync()).Where(ca => ca.CategoryId == id).ToList();
            foreach (var ca in categoryAssets)
            {
                await _bll.CategoryAssetsService.RemoveAsync(ca.Id);
            }
            await _bll.CategoryService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
