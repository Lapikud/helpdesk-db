using App.BLL.Contracts;
using App.DTO.v1;
using App.DTO.v1.CreateObjects;
using App.DTO.v1.UpdateObjects;
using App.DTO.v1.ViewModels;
using Asp.Versioning;
using Base.Contracts;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels;
using AssetsOverviewViewModel = App.DTO.v1.ViewModels.AssetsOverviewViewModel;

namespace WebApp.ApiControllers
{
    /// <summary>
    /// API controller for managing assets overview.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class HomeController : ControllerBase
    {
        private readonly IAppBLL _bll;
        private readonly IUserNameResolver _userNameResolver;

        private readonly App.DTO.v1.Mappers.AssetViewModelMapper _mapper =
            new App.DTO.v1.Mappers.AssetViewModelMapper();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bll">Business Logic.</param>
        /// <param name="userNameResolver">Username Resolver.</param>
        public HomeController(IAppBLL bll, IUserNameResolver userNameResolver)
        {
            _bll = bll;
            _userNameResolver = userNameResolver;
        }

        /// <summary>
        /// Get an overview.
        /// </summary>
        /// <returns></returns>
        [HttpGet("overview")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.ViewModels.AssetsOverviewViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssetsOverviewViewModel>> GetOverview([FromQuery] string? searchTerm)
        {
            var userId = User.GetUserId();

            var availableAssets = (await _bll.AssetService.GetAvailableAssets())
                .Select(avm => _mapper.Map(avm)!)
                .ToList();
            var assetsReservedByUser = (await _bll.AssetService.GetAssetsReservedByUser(userId))
                .Select(avm => _mapper.Map(avm)!)
                .ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                availableAssets = availableAssets.Where(a =>
                    a.AssetName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.OwnerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (a.SerialNumber != null && a.SerialNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Barcode != null && a.Barcode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            var vm = new AssetsOverviewViewModel
            {
                AvailableAssets = availableAssets,
                AssetsReservedByUser = assetsReservedByUser
            };

            return Ok(vm);
        }

        /// <summary>
        /// Create an asset with category, location and owner.
        /// </summary>
        /// <param name="assetVm">The asset to create.</param>
        /// <returns>The created asset.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost("overview/createNewAsset")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(App.DTO.v1.ViewModels.AssetViewModel), StatusCodes.Status201Created)]
        public async Task<ActionResult<AssetViewModel>> CreateNewAsset(
            App.DTO.v1.CreateObjects.AssetViewModelCreate assetVm)
        {
            var bllEntityAsset = await _bll.AssetService.CreateNewAsset(assetVm.AssetName, assetVm.Comment, assetVm.SerialNumber, assetVm.Barcode);

            await _bll.CategoryAssetsService.CreateNewCategoryAsset(bllEntityAsset.Id, assetVm.SelectedCategoryId);

            await _bll.OwnerAssetsService.CreateNewOwnerAsset(bllEntityAsset.Id, assetVm.SelectedOwnerId);

            await _bll.LocationAssetsService.CreateNewLocationAsset(bllEntityAsset.Id, assetVm.SelectedLocationId);

            await _bll.SaveChangesAsync();

            var addedAssetViewModel = _mapper.Map(await _bll.AssetService.GetAssetVmByAssetId(bllEntityAsset.Id));

            return CreatedAtAction("GetOverview", new
            {
                id = bllEntityAsset.Id,
                version = HttpContext.RequestedApiVersion!.ToString()
            }, addedAssetViewModel);
        }

        /// <summary>
        /// Update an asset, its category, owner and location.
        /// </summary>
        /// <param name="id">The id of the asset to update.</param>
        /// <param name="assetVm">The updated asset data.</param>
        /// <returns>A status indicating the result of the update operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPut("overview/edit/{id:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> EditAsset(Guid id, AssetViewModelUpdate assetVm)
        {
            if (!id.Equals(assetVm.AssetId))
            {
                return BadRequest();
            }

            var asset = await _bll.AssetService.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            try
            {
                asset.AssetName = assetVm.AssetName;
                asset.Comment = assetVm.Comment;
                asset.SerialNumber = assetVm.SerialNumber;
                asset.Barcode = assetVm.Barcode;

                await _bll.AssetService.UpdateAsync(asset);
                await _bll.CategoryAssetsService.UpdateCategoryOfAsset(assetVm.CategoryAssetsId,
                    assetVm.SelectedCategoryId, assetVm.AssetId);
                await _bll.OwnerAssetsService.UpdateOwnerOfAsset(assetVm.OwnerAssetsId, assetVm.SelectedOwnerId,
                    assetVm.AssetId);
                await _bll.LocationAssetsService.UpdateLocationOfAsset(assetVm.LocationAssetsId,
                    assetVm.SelectedLocationId, assetVm.AssetId);

                await _bll.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _bll.AssetService.ExistsAsync(id) ||
                    await _bll.CategoryAssetsService.ExistsAsync(assetVm.SelectedCategoryId) ||
                    await _bll.OwnerAssetsService.ExistsAsync(assetVm.SelectedOwnerId) ||
                    await _bll.LocationAssetsService.ExistsAsync(assetVm.SelectedLocationId))
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
        /// Remove the current (or soonest upcoming) reservation of an asset for the current user.
        /// </summary>
        /// <param name="id">The id of the asset whose reservation to remove.</param>
        /// <returns>A status indicating the result of the remove operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost("overview/remove-reservation/{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveReservation(Guid id)
        {
            var asset = await _bll.AssetService.FindAsync(id);
            if (asset == null) return NotFound();

            await _bll.AssetReservationService.RemoveAssetReservation(User.GetUserId(), asset.Id);
            await _bll.SaveChangesAsync();

            return Ok(new Message($"Reservation for asset: '{asset.Id}' was removed."));
        }

        /// <summary>
        /// Remove asset
        /// </summary>
        /// <param name="id">The id of the asset to remove.</param>
        /// <param name="removeAssetVm">The data of removing asset.</param>
        /// <returns>A status indicating the result of the removed asset create operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost("overview/remove/{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(Guid id, App.DTO.v1.CreateObjects.RemovedAssetsCreate removeAssetVm)
        {
            if (id != removeAssetVm.AssetId) return NotFound();

            var asset = await _bll.AssetService.FindAsync(removeAssetVm.AssetId);
            if (asset == null) return NotFound();

            var removedAsset = await _bll.RemovedAssetsService.CreateNewRemovedAsset(asset.Id, removeAssetVm.Comment);
            if (removedAsset == null)
            {
                return BadRequest(new Message($"Asset: {asset.AssetName} is already removed"));
            }

            await _bll.SaveChangesAsync();

            return Ok(new Message($"Asset: '{asset.Id}' was removed."));
        }


        /// <summary>
        /// Mark the asset as returned, closing the user's active reservation.
        /// If returned early, sets ReservationTo to the current time.
        /// </summary>
        /// <param name="id">The id of the asset being returned.</param>
        /// <returns>A status indicating the result.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost("overview/return/{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReturnAsset(Guid id)
        {
            var asset = await _bll.AssetService.FindAsync(id);
            if (asset == null) return NotFound();

            await _bll.AssetReservationService.AssetReturned(User.GetUserId(), asset.Id);
            await _bll.SaveChangesAsync();

            return Ok(new Message($"Asset '{asset.AssetName}' marked as returned."));
        }

        /// <summary>
        /// Reserve asset
        /// </summary>
        /// <param name="id">The id of the asset to reserve.</param>
        /// <param name="assetReservationVm">The data of asset reservation.</param>
        /// <returns>A status indicating the result of the create operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPost("overview/reserve/{id:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reserve(Guid id,
            App.DTO.v1.CreateObjects.AssetReservationCreate assetReservationVm)
        {
            if (id != assetReservationVm.AssetId) return NotFound();

            var asset = await _bll.AssetService.FindAsync(assetReservationVm.AssetId);
            if (asset == null) return NotFound();

            if (await _bll.AssetReservationService.HasActiveOrFutureReservation(id, User.GetUserId()))
            {
                return BadRequest(new Message(
                    $"User: {_userNameResolver.CurrentUserName} has active reservation for asset: {asset.AssetName}"));
            }

            // Convert to UTC safely: handles both Local and Unspecified (assumes Local)
            var reservationFrom = assetReservationVm.ReservationFrom.ToUniversalTime();
            var reservationTo = assetReservationVm.ReservationTo.ToUniversalTime();

            var isAvailable = await _bll.AssetReservationService.IsAssetReservationAvailable(
                assetReservationVm.AssetId,
                reservationFrom,
                reservationTo
            );
            if (!isAvailable)
            {
                return BadRequest(new Message(
                    $"Asset: {asset.AssetName} reservation for: {reservationFrom:u} – {reservationTo:u} is not available"));
            }

            await _bll.AssetReservationService.UserReserveAsset(
                assetReservationVm.UserId,
                assetReservationVm.AssetId,
                reservationFrom,
                reservationTo
            );

            await _bll.SaveChangesAsync();

            return Ok(new Message($"Asset: '{asset.Id}' was reserved."));
        }

        /// <summary>
        /// Change asset reservation time
        /// </summary>
        /// <param name="reservationId">The id of the asset reservation to change.</param>
        /// <param name="assetReservationVm">The data of changed asset reservation.</param>
        /// <returns>A status indicating the result of the change operation.</returns>
        [Authorize(Roles = "admins,helpdesk_db_admins,members,pixels")]
        [HttpPut("overview/changeReservationTime/{reservationId:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeReservationTime(Guid reservationId,
            App.DTO.v1.UpdateObjects.AssetReservationUpdate assetReservationVm)
        {
            if (!reservationId.Equals(assetReservationVm.AssetReservationId))
            {
                return NotFound();
            }

            var existing = await _bll.AssetReservationService.FindAsync(reservationId);
            if (existing == null || !existing.UserId.Equals(User.GetUserId()))
            {
                return NotFound();
            }

            var reservationFrom = assetReservationVm.ReservationFrom.ToUniversalTime();
            var reservationTo = assetReservationVm.ReservationTo.ToUniversalTime();

            var isAvailable = await _bll.AssetReservationService.IsAssetReservationAvailable(
                assetReservationVm.AssetId,
                reservationFrom,
                reservationTo,
                reservationId
            );
            if (!isAvailable)
            {
                return BadRequest(new Message(
                    $"Reservation for asset: {existing.Asset!.AssetName} for {reservationFrom:u} – {reservationTo:u} is not available"));
            }

            existing.ReservationFrom = reservationFrom;
            existing.ReservationTo = reservationTo;
            await _bll.AssetReservationService.UpdateAsync(existing);

            await _bll.SaveChangesAsync();

            return NoContent();
        }
    }
}