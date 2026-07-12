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
    /// API controller for managing categories.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class CupboardsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.CupboardMapper _mapper =
            new App.DTO.v1.Mappers.CupboardMapper();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public CupboardsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all cupboards.
        /// </summary>
        /// <returns>List of cupboards.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Cupboard>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.Cupboard>>> GetCupboards()
        {
            var data = await _bll.CupboardService.AllAsync();
            var res = data.Select(c => _mapper.Map(c)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a cupboard by id.
        /// </summary>
        /// <param name="id">The id of the cupboard.</param>
        /// <returns>The cupboard.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Cupboard), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Cupboard>> GetCupboard(Guid id)
        {
            var cupboard = await _bll.CupboardService.FindAsync(id);

            if (cupboard == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(cupboard)!);
        }

        /// <summary>
        /// Update a cupboard.
        /// </summary>
        /// <param name="id">The id of the cupboard to update.</param>
        /// <param name="cupboard">The updated cupboard data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutCupboard(Guid id, App.DTO.v1.Cupboard cupboard)
        {
            if (id != cupboard.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.CupboardService.UpdateAsync(_mapper.Map(cupboard)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.CupboardService.ExistsAsync(id))
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
        /// Create a cupboard.
        /// </summary>
        /// <param name="cupboard">The cupboard to create.</param>
        /// <returns>The created cupboard.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Cupboard>), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Cupboard>> PostCupboard(App.DTO.v1.CreateObjects.CupboardCreate cupboard)
        {
            var bllEntity = _mapper.Map(cupboard);
            await _bll.CupboardService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoCupboard = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetCupboard", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoCupboard);
        }

        /// <summary>
        /// Delete a cupboard.
        /// </summary>
        /// <param name="id">The id of the cupboard to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCupboard(Guid id)
        {
            var cupboardInRooms = (await _bll.CupboardInRoomService.AllAsync()).Where(ca => ca.CupboardId == id).ToList();
            var locationsInCupboards = (await _bll.LocationInCupboardService.AllAsync()).Where(ca => ca.CupboardId == id).ToList();
            foreach (var cir in cupboardInRooms)
            {
                await _bll.CupboardInRoomService.RemoveAsync(cir.Id);
            }
            foreach (var lic in locationsInCupboards)
            {
                await _bll.LocationInCupboardService.RemoveAsync(lic.Id);
            }
            await _bll.CupboardService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
