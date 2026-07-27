import { BaseService } from "./BaseService";
import { AccountService } from "./AccountService";
import { AssetReservationService } from "./AssetReservationService";
import { AssetService } from "./AssetService";
import { CategoryAssetsService } from "./CategoryAssetsService";
import { CategoryService } from "./CategoryService";
import { CupboardService } from "./CupboardService";
import { CupboardsInRoomsService } from "./CupboardsInRoomsService";
import { LocationAssetsService } from "./LocationAssetsService";
import { LocationInCupboardService } from "./LocationInCupboardService";
import { LocationService } from "./LocationService";
import { OverviewService } from "./OverviewService";
import { OwnerAssetsService } from "./OwnerAssetsService";
import { OwnerService } from "./OwnerService";
import { RefreshTokenService } from "./RefreshTokenService";
import { RemovedAssetsService } from "./RemovedAssetsService";
import { RoleService } from "./RoleService";
import { RoomService } from "./RoomService";
import { UserManagementService } from "./UserManagementService";
import { UserRoleService } from "./UserRoleService";
import { UserService } from "./UserService";

/**
 * Shared service instances.
 *
 * Services hold no per-page state — only a fixed `basePath` and an axios
 * instance — and the one piece of cross-instance state (the coalesced refresh
 * promise) already lives at module scope in BaseService. So a single instance
 * per service is equivalent to the old per-page `useMemo(() => new X(), [])`,
 * minus an axios instance + interceptor per mount.
 *
 * `axios.create()` at import time is inert (no network, no browser globals),
 * so this is safe to evaluate during SSR.
 *
 * `injectSetAccountInfo` is wired once by ServiceAuthBinder — see
 * `src/components/ServiceAuthBinder.tsx`.
 */
export const accountService = new AccountService();
export const assetReservationService = new AssetReservationService();
export const assetService = new AssetService();
export const categoryAssetsService = new CategoryAssetsService();
export const categoryService = new CategoryService();
export const cupboardService = new CupboardService();
export const cupboardsInRoomsService = new CupboardsInRoomsService();
export const locationAssetsService = new LocationAssetsService();
export const locationInCupboardService = new LocationInCupboardService();
export const locationService = new LocationService();
export const overviewService = new OverviewService();
export const ownerAssetsService = new OwnerAssetsService();
export const ownerService = new OwnerService();
export const refreshTokenService = new RefreshTokenService();
export const removedAssetsService = new RemovedAssetsService();
export const roleService = new RoleService();
export const roomService = new RoomService();
export const userManagementService = new UserManagementService();
export const userRoleService = new UserRoleService();
export const userService = new UserService();

/** Every service instance, for one-shot wiring of the account-info setter. */
export const allServices: BaseService[] = [
	accountService,
	assetReservationService,
	assetService,
	categoryAssetsService,
	categoryService,
	cupboardService,
	cupboardsInRoomsService,
	locationAssetsService,
	locationInCupboardService,
	locationService,
	overviewService,
	ownerAssetsService,
	ownerService,
	refreshTokenService,
	removedAssetsService,
	roleService,
	roomService,
	userManagementService,
	userRoleService,
	userService,
];
