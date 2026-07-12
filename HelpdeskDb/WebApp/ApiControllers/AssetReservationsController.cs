using App.BLL.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Base.Helpers;
using App.DTO.v1;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing assets reservations.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AssetReservationsController : ControllerBase
    {
        private readonly IAppBLL _bll;

        private readonly App.DTO.v1.Mappers.AssetReservationMapper _mapper =
            new App.DTO.v1.Mappers.AssetReservationMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public AssetReservationsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all assets reservations.
        /// </summary>
        /// <returns>List of assets reservations</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.AssetReservation>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.AssetReservation>>> GetAssetReservations()
        {
            var data = await _bll.AssetReservationService.AllAsync();
            var res = data.Select(ar => _mapper.Map(ar)!).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get a asset reservation by id.
        /// </summary>
        /// <param name="id">The id of the asset reservation.</param>
        /// <returns>The asset reservation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.AssetReservation), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.AssetReservation>> GetAssetReservation(Guid id)
        {
            var assetReservation = await _bll.AssetReservationService.FindAsync(id);

            if (assetReservation == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(assetReservation)!);
        }

        /// <summary>
        /// Update a asset reservation.
        /// </summary>
        /// <param name="id">The id of the asset reservation to update.</param>
        /// <param name="assetReservation">The updated asset reservation data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutAssetReservation(Guid id, App.DTO.v1.AssetReservation assetReservation)
        {
            if (id != assetReservation.Id)
            {
                return BadRequest();
            }

            try
            {
                var reservationFrom = assetReservation.ReservationFrom.ToUniversalTime();
                var reservationTo = assetReservation.ReservationTo.ToUniversalTime();
                
                var isAvailable = await _bll.AssetReservationService.IsAssetReservationAvailable(
                    assetReservation.AssetId,
                    reservationFrom,
                    reservationTo,
                    id
                );

                var existing = await _bll.AssetReservationService.FindAsync(assetReservation.Id);
                if (existing == null || !existing.UserId.Equals(User.GetUserId()))
                {
                    return NotFound();
                }
                        
                if (!isAvailable)
                {
                    return BadRequest(new Message(
                    $"Reservation for asset: {existing.Asset!.AssetName} for {reservationFrom:u} – {reservationTo:u} is not available"));
                }

                assetReservation.ReservationFrom = reservationFrom;
                assetReservation.ReservationTo = reservationTo;
                
                await _bll.AssetReservationService.UpdateAsync(_mapper.Map(assetReservation)! /*, assetReservation.UserId*/);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.AssetReservationService.ExistsAsync(id))
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
        /// Create an asset reservation.
        /// </summary>
        /// <param name="assetReservation">The asset reservation to create.</param>
        /// <returns>The created asset reservation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.AssetReservation), StatusCodes.Status201Created)]
        public async Task<ActionResult<App.DTO.v1.AssetReservation>> PostAssetReservation(App.DTO.v1.CreateObjects.AssetReservationCreate assetReservation)
        {
            var reservationFrom = assetReservation.ReservationFrom.ToUniversalTime();
            var reservationTo = assetReservation.ReservationTo.ToUniversalTime();
            
            var isAvailable = await _bll.AssetReservationService.IsAssetReservationAvailable(
                assetReservation.AssetId,
                reservationFrom,
                reservationTo
            );

            if (!isAvailable)
            {
                // Return a specific error message that the frontend can display
                return BadRequest(new { Message = "Asset is already reserved for the selected time period." });
            }
            
            assetReservation.ReservationFrom = reservationFrom;
            assetReservation.ReservationTo = reservationTo;

            var bllEntity = _mapper.Map(assetReservation);
            await _bll.AssetReservationService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoAssetReservation = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetAssetReservation", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoAssetReservation);
        }

        /// <summary>
        /// Delete a asset reservation.
        /// </summary>
        /// <param name="id">The id of the asset reservation to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAssetReservation(Guid id)
        {
            var existing = await _bll.AssetReservationService.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Owners may delete their own reservation; admins may delete any.
            var isAdmin = User.IsInRole("admins") || User.IsInRole("helpdesk_db_admins");
            if (!isAdmin && !existing.UserId.Equals(User.GetUserId()))
            {
                return NotFound();
            }

            await _bll.AssetReservationService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
