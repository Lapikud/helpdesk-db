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
    /// API controller for managing assets.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AssetsController : ControllerBase
    {
        private readonly IAppBLL _bll;
        
        private readonly App.DTO.v1.Mappers.AssetMapper _mapper =
            new App.DTO.v1.Mappers.AssetMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        public AssetsController(IAppBLL bll)
        {
            _bll = bll;
        }

        /// <summary>
        /// Get all assets.
        /// </summary>
        /// <returns>List of assets.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<App.DTO.v1.Asset>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets([FromQuery] bool includeRemoved = false)
        {
            var data = includeRemoved 
                ? await _bll.AssetService.AllAsync() 
                : await _bll.AssetService.GetAllNotRemovedAssets();
            var res = data.Select(c => _mapper.Map(c)!).OrderBy(a => a.AssetName).ToList();
            return Ok(res);
        }

        /// <summary>
        /// Get an asset by id.
        /// </summary>
        /// <param name="id">The id of the asset.</param>
        /// <returns>The asset.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpGet("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Asset), StatusCodes.Status200OK)]
        public async Task<ActionResult<Asset>> GetAsset(Guid id)
        {
            var asset = await _bll.AssetService.FindAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map(asset)!);
        }

        /// <summary>
        /// Update an asset.
        /// </summary>
        /// <param name="id">The id of the asset to update.</param>
        /// <param name="asset">The updated asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPut("{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutAsset(Guid id, Asset asset)
        {
            if (id != asset.Id)
            {
                return BadRequest();
            }

            try
            {
                await _bll.AssetService.UpdateAsync(_mapper.Map(asset)!);
                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.AssetService.ExistsAsync(id))
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
        /// Create an asset.
        /// </summary>
        /// <param name="asset">The asset to create.</param>
        /// <returns>The created asset.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.Asset), StatusCodes.Status201Created)]
        public async Task<ActionResult<Asset>> PostAsset(App.DTO.v1.CreateObjects.AssetCreate asset)
        {
            var bllEntity = _mapper.Map(asset);
            await _bll.AssetService.AddAsync(bllEntity);
            await _bll.SaveChangesAsync();

            var dtoAsset = _mapper.Map(bllEntity)!;
            return CreatedAtAction("GetAsset", new
            {
                id = bllEntity.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, dtoAsset);
        }

        /// <summary>
        /// Delete an asset.
        /// </summary>
        /// <param name="id">The id of the asset to delete.</param>
        /// <returns>A status indicating the result of the delete operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins")]
        [HttpDelete("{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            var locationAssets = (await _bll.LocationAssetsService.AllAsync()).Where(la => la.AssetId == id).ToList();
            var categoryAssets = (await _bll.CategoryAssetsService.AllAsync()).Where(ca => ca.AssetId == id).ToList();
            var ownerAssets = (await _bll.OwnerAssetsService.AllAsync()).Where(oa => oa.AssetId == id).ToList();
            var removedAssets = (await _bll.RemovedAssetsService.AllAsync()).Where(ra => ra.AssetId == id).ToList();
            var assetReservations = (await _bll.AssetReservationService.AllAsync()).Where(ar => ar.AssetId == id).ToList();
            foreach (var la in locationAssets)
            {
                await _bll.LocationAssetsService.RemoveAsync(la.Id);
            }
            foreach (var ca in categoryAssets)
            {
                await _bll.CategoryAssetsService.RemoveAsync(ca.Id);
            }
            foreach (var oa in ownerAssets)
            {
                await _bll.OwnerAssetsService.RemoveAsync(oa.Id);
            }
            foreach (var ra in removedAssets)
            {
                await _bll.RemovedAssetsService.RemoveAsync(ra.Id);
            }
            foreach (var ar in assetReservations)
            {
                await _bll.AssetReservationService.RemoveAsync(ar.Id, ar.UserId);
            }
            await _bll.AssetService.RemoveAsync(id);
            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}
