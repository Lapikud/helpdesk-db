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
    /// API controller for managing rooms.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admins,helpdesk_db_admins")]
    public class RoomsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.RoomMapper _mapper =
            new App.DTO.v1.Mappers.RoomMapper();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public RoomsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all rooms.
        /// </summary>
        /// <returns>List of rooms.</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Room>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.Room>>> GetRooms()
        {
            var data = await _bll.RoomService.AllAsync();
            var res = data.Select(r => _mapper.Map(r)!).OrderBy(r => r.RoomName).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a room by id.
        /// </summary>
        /// <param name="id">The id of the room.</param>
        /// <returns>The room.</returns>
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Room), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Room>> GetRoom(Guid id)
        {
            var room = await _bll.RoomService.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(room)!);
        }

        /// <summary>
        /// Update a room.
        /// </summary>
        /// <param name="id">The id of the room to update.</param>
        /// <param name="room">The updated room data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutRoom(Guid id, App.DTO.v1.Room room)
        {
            if (id != room.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.RoomService.UpdateAsync(_mapper.Map(room)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.RoomService.ExistsAsync(id))
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
        /// Create a room.
        /// </summary>
        /// <param name="room">The room to create.</param>
        /// <returns>The created room.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Room), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.Room>> PostRoom(App.DTO.v1.CreateObjects.RoomCreate room)
        {
            var bllEntity = _mapper.Map(room);
            await _bll.RoomService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoRoom = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetRoom", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoRoom);
        }

        /// <summary>
        /// Delete a room.
        /// </summary>
        /// <param name="id">The id of the room to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            var cupboardInRooms = (await _bll.CupboardInRoomService.AllAsync()).Where(ca => ca.RoomId == id).ToList();
            foreach (var cir in cupboardInRooms)
            {
                await _bll.CupboardInRoomService.RemoveAsync(cir.Id);
            }
            await _bll.RoomService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
