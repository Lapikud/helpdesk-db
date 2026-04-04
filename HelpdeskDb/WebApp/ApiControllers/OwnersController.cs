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
    /// API controller for managing owners.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OwnersController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.OwnerMapper _mapper =
            new App.DTO.v1.Mappers.OwnerMapper();

        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public OwnersController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all owners.
        /// </summary>
        /// <returns>List of owners.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Owner>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<App.DTO.v1.Owner>>> GetOwners()
        {
            var data = await _bll.OwnerService.AllAsync();
            var res = data.Select(o => _mapper.Map(o)!).OrderBy(o => o.OwnerName).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get an owner by id.
        /// </summary>
        /// <param name="id">The id of the owner.</param>
        /// <returns>The owner.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Owner), StatusCodes.Status200OK)]
        public async Task<ActionResult<App.DTO.v1.Owner>> GetOwner(Guid id)
        {
            var owner = await _bll.OwnerService.FindAsync(id);

            if (owner == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(owner)!);
        }

        /// <summary>
        /// Update an owner.
        /// </summary>
        /// <param name="id">The id of the owner to update.</param>
        /// <param name="owner">The updated owner data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutOwner(Guid id, Owner owner)
        {
            if (id != owner.Id)
            {
                return BadRequest();
            }
            
            var owners = await _bll.OwnerService.AllAsync();
            if (owners.Any(c => c.OwnerName.Equals(owner.OwnerName, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new Message(App.Resources.Errors.ValidationErrors.OwnerNameExists));
            }

            try
            {
                await _bll.OwnerService.UpdateAsync(_mapper.Map(owner)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.OwnerService.ExistsAsync(id))
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
        /// Create an owner.
        /// </summary>
        /// <param name="owner">The owner to create.</param>
        /// <returns>The created owner.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Owner), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Owner>> PostOwner(App.DTO.v1.CreateObjects.OwnerCreate owner)
        {
            var owners = await _bll.OwnerService.AllAsync();
            if (owners.Any(c => c.OwnerName.Equals(owner.OwnerName, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new Message(App.Resources.Errors.ValidationErrors.OwnerNameExists));
            }
            
            var bllEntity = _mapper.Map(owner);
            await _bll.OwnerService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();
            
            var dtoOwner = _mapper.Map(bllEntity)!;

            return CreatedAtAction("GetOwner", new
            {
                id = bllEntity.Id,
                version = HttpContext.GetRequestedApiVersion()!.ToString()
            }, dtoOwner);
        }

        /// <summary>
        /// Delete an owner.
        /// </summary>
        /// <param name="id">The id of the owner to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOwner(Guid id)
        {
            var ownerAssets = (await _bll.OwnerAssetsService.AllAsync()).Where(ca => ca.OwnerId == id).ToList();
            foreach (var oa in ownerAssets)
            {
                await _bll.OwnerAssetsService.RemoveAsync(oa.Id);
            }
            await _bll.OwnerService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
