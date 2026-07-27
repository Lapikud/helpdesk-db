"use client";

import { useQuery } from "@tanstack/react-query";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	assetReservationService,
	assetService,
	categoryAssetsService,
	categoryService,
	cupboardService,
	cupboardsInRoomsService,
	locationAssetsService,
	locationInCupboardService,
	locationService,
	ownerAssetsService,
	ownerService,
	refreshTokenService,
	removedAssetsService,
	roleService,
	roomService,
	userRoleService,
	userService,
} from "@/services";

/**
 * Shared list queries.
 *
 * Defining these once (rather than inlining `useQuery` per page) is what makes
 * the cache actually shared: five pages call `useAssets(true)` and the request
 * fires once. Reference data that rarely changes gets a longer staleTime;
 * transactional data uses the 30s client default.
 */
const REFERENCE_DATA_STALE_TIME = 5 * 60_000;

// ---------------------------------------------------------------- reference

export const useCategories = () =>
	useQuery({
		queryKey: qk.categories(),
		queryFn: () => unwrap(categoryService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

export const useOwners = () =>
	useQuery({
		queryKey: qk.owners(),
		queryFn: () => unwrap(ownerService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

export const useRooms = () =>
	useQuery({
		queryKey: qk.rooms(),
		queryFn: () => unwrap(roomService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

export const useCupboards = () =>
	useQuery({
		queryKey: qk.cupboards(),
		queryFn: () => unwrap(cupboardService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

export const useLocations = (options: { enabled?: boolean } = {}) =>
	useQuery({
		queryKey: qk.locations(),
		queryFn: () => unwrap(locationService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
		enabled: options.enabled ?? true,
	});

export const useRoles = () =>
	useQuery({
		queryKey: qk.roles(),
		queryFn: () => unwrap(roleService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

export const useUsers = () =>
	useQuery({
		queryKey: qk.users(),
		queryFn: () => unwrap(userService.getAllAsync()),
		staleTime: REFERENCE_DATA_STALE_TIME,
	});

// ------------------------------------------------------------ transactional

/**
 * `includeRemoved` is part of the key because the two variants return
 * different lists — sharing one entry would let whichever page mounted last
 * overwrite the other's data.
 */
export const useAssets = (
	includeRemoved = false,
	options: { enabled?: boolean } = {},
) =>
	useQuery({
		queryKey: qk.assets(includeRemoved),
		queryFn: () => unwrap(assetService.getAllAsync(includeRemoved)),
		enabled: options.enabled ?? true,
	});

export const useAssetReservations = () =>
	useQuery({
		queryKey: qk.assetReservations(),
		queryFn: () => unwrap(assetReservationService.getAllAsync()),
	});

export const useRemovedAssets = () =>
	useQuery({
		queryKey: qk.removedAssets(),
		queryFn: () => unwrap(removedAssetsService.getAllAsync()),
	});

export const useCategoryAssets = () =>
	useQuery({
		queryKey: qk.categoryAssets(),
		queryFn: () => unwrap(categoryAssetsService.getAllAsync()),
	});

export const useLocationAssets = () =>
	useQuery({
		queryKey: qk.locationAssets(),
		queryFn: () => unwrap(locationAssetsService.getAllAsync()),
	});

export const useOwnerAssets = () =>
	useQuery({
		queryKey: qk.ownerAssets(),
		queryFn: () => unwrap(ownerAssetsService.getAllAsync()),
	});

export const useCupboardsInRooms = () =>
	useQuery({
		queryKey: qk.cupboardsInRooms(),
		queryFn: () => unwrap(cupboardsInRoomsService.getAllAsync()),
	});

export const useLocationsInCupboards = (
	options: { enabled?: boolean } = {},
) =>
	useQuery({
		queryKey: qk.locationsInCupboards(),
		queryFn: () => unwrap(locationInCupboardService.getAllAsync()),
		enabled: options.enabled ?? true,
	});

export const useUserRoles = () =>
	useQuery({
		queryKey: qk.userRoles(),
		queryFn: () => unwrap(userRoleService.getAllAsync()),
	});

export const useRefreshTokens = () =>
	useQuery({
		queryKey: qk.refreshTokens(),
		queryFn: () => unwrap(refreshTokenService.getAllAsync()),
	});

// -------------------------------------------------------------- single entity

/**
 * Per-id lookups, used by the overview page's dialogs.
 *
 * Each takes a nullable id and stays disabled until one is set, so the query
 * is declared unconditionally (hooks can't be called conditionally) but only
 * fires once a row is selected. The `id!` inside `queryFn` is safe precisely
 * because `enabled` gates it.
 */
export const useAsset = (id: string | null) =>
	useQuery({
		queryKey: qk.asset(id ?? ""),
		queryFn: () => unwrap(assetService.getAsync(id!)),
		enabled: !!id,
	});

export const useAssetReservation = (id: string | null) =>
	useQuery({
		queryKey: qk.assetReservation(id ?? ""),
		queryFn: () => unwrap(assetReservationService.getAsync(id!)),
		enabled: !!id,
	});

/**
 * The three `…ByAssetId` service methods unwrap internally: they resolve to
 * `null` only when the asset genuinely has no mapping yet, and reject with
 * `ApiError` on a failed fetch — so a failure marks the query errored instead
 * of masquerading as "no mapping".
 */
export const useCategoryAssetByAsset = (assetId: string | null) =>
	useQuery({
		queryKey: qk.categoryAssetByAsset(assetId ?? ""),
		queryFn: () =>
			categoryAssetsService.getCategoryAssetByAssetId(assetId!),
		enabled: !!assetId,
	});

export const useLocationAssetByAsset = (assetId: string | null) =>
	useQuery({
		queryKey: qk.locationAssetByAsset(assetId ?? ""),
		queryFn: () =>
			locationAssetsService.getLocationAssetByAssetId(assetId!),
		enabled: !!assetId,
	});

export const useOwnerAssetByAsset = (assetId: string | null) =>
	useQuery({
		queryKey: qk.ownerAssetByAsset(assetId ?? ""),
		queryFn: () => ownerAssetsService.getOwnerAssetByAssetId(assetId!),
		enabled: !!assetId,
	});
