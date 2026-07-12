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
    /// API controller for managing cupboards in rooms.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class CupboardsInRoomsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.CupboardInRoomMapper _mapper =
            new App.DTO.v1.Mappers.CupboardInRoomMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business logic.</param>
        public CupboardsInRoomsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all cupboards in rooms.
        /// </summary>
        /// <returns>List of cupboards in rooms.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.CupboardInRoom>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.CupboardInRoom>>> GetCupboardsInRooms()
        {
            var data = await _bll.CupboardInRoomService.AllAsync();
            var res = data.Select(cir => _mapper.Map(cir)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a cupboard in the room by id.
        /// </summary>
        /// <param name="id">The id of the cupboard in the room.</param>
        /// <returns>The cupboard in the room.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.CupboardInRoom), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.CupboardInRoom>> GetCupboardInRoom(Guid id)
        {
            var cupboardInRoom = await _bll.CupboardInRoomService.FindAsync(id);

            if (cupboardInRoom == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(cupboardInRoom)!);
        }

        /// <summary>
        /// Update a cupboard in a room.
        /// </summary>
        /// <param name="id">The id of the cupboard in the room to update.</param>
        /// <param name="cupboardInRoom">The updated cupboard in the room data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutCupboardInRoom(Guid id, App.DTO.v1.CupboardInRoom cupboardInRoom)
        {
            if (id != cupboardInRoom.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.CupboardInRoomService.UpdateAsync(_mapper.Map(cupboardInRoom)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.CupboardInRoomService.ExistsAsync(id))
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
        /// Create a cupboard in a room.
        /// </summary>
        /// <param name="cupboardInRoom">The cupboard in the room to create.</param>
        /// <returns>The created cupboard in the room.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.CupboardInRoom), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.CupboardInRoom>> PostCupboardInRoom(App.DTO.v1.CreateObjects.CupboardInRoomCreate cupboardInRoom)
        {
            var bllEntity = _mapper.Map(cupboardInRoom);
            await _bll.CupboardInRoomService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoCupboardInRoom = _mapper.Map(bllEntity)!;
            
            return CreatedAtAction("GetCupboardInRoom", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion?.ToString()
            }, dtoCupboardInRoom);
        }

        /// <summary>
        /// Delete a cupboard in a room.
        /// </summary>
        /// <param name="id">The id of the cupboard in the room to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCupboardInRoom(Guid id)
        {
            await _bll.CupboardInRoomService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
