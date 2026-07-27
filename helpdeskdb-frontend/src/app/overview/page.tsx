"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { overviewService } from "@/services";
import { useRouter, useSearchParams } from "next/navigation";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useOverview } from "@/hooks/queries/overviewQueries";
import {
	useAsset,
	useAssetReservation,
	useCategories,
	useCategoryAssetByAsset,
	useLocationAssetByAsset,
	useLocations,
	useOwnerAssetByAsset,
	useOwners,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	IAssetViewModel,
	IAssetViewModelCreate,
	IAssetViewModelRemove,
	IAssetViewModelUpdate,
} from "@/types/domain/IAssetViewModels";
import {
	IAssetReservationWithNames,
	IAssetReservationAdd,
	IAssetReservationUpdate,
} from "@/types/domain/DomainTypes";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import AssetList from "@/components/AssetList";
import { useBarcodeScanner } from "@/hooks/useBarcodeScanner";
import { EditAssetDialog } from "@/components/dialogs/overviewDialogs/EditAssetDialog";
import { RemoveAssetDialog } from "@/components/dialogs/overviewDialogs/RemoveAssetDialog";
import Spinner from "@/components/LoadingSpinner";
import { CreateAssetDialog } from "@/components/dialogs/overviewDialogs/CreateAssetDialog";
import { ReserveAssetDialog } from "@/components/dialogs/overviewDialogs/ReserveAssetDialog";
import { ChangeReservationTimeDialog } from "@/components/dialogs/overviewDialogs/ChangeReservationTimeDialog";
import { RemoveReservationDialog } from "@/components/dialogs/overviewDialogs/RemoveReservationDialog";

export default function Overview() {
	const { t: tAssetViewModel } = useTranslation("assetviewmodel");
	const { t: tCommon } = useTranslation("common");
	const { accountInfo } = useContext(AccountContext);
	const router = useRouter();
	const searchParams = useSearchParams();

	const [selectedMode, setSelectedMode] = useState<string>("lines");

	// The URL is the source of truth for the search term; `searchInput` is just
	// the uncontrolled field's draft until it is submitted.
	const searchTerm = searchParams.get("searchTerm") || "";
	const [searchInput, setSearchInput] = useState(searchTerm);

	useEffect(() => {
		setSearchInput(searchTerm);
	}, [searchTerm]);

	const overview = useOverview(searchTerm);
	const availableAssets = overview.data?.availableAssets ?? [];
	const assetsReservedByUser = overview.data?.assetsReservedByUser ?? [];

	// Reference data for the create/edit dropdowns. Shared with /categories,
	// /locations and /owners through the cache, and held for 5 minutes, so the
	// dialogs open with the lists already in hand.
	const categoriesQuery = useCategories();
	const locationsQuery = useLocations();
	const ownersQuery = useOwners();
	const categories = categoriesQuery.data ?? [];
	const locations = locationsQuery.data ?? [];
	const owners = ownersQuery.data ?? [];

	// --------------------------------------------------------------- dialogs
	//
	// Each dialog is driven by the entity it acts on rather than a separate
	// `show…` flag: selecting a row enables that dialog's queries, and the
	// dialog opens once they have resolved. That keeps the row spinner and the
	// dialog's data in lockstep without an imperative fetch-then-open step.

	const [showCreateModal, setShowCreateModal] = useState(false);
	const [assetToUpdate, setAssetToUpdate] = useState<IAssetViewModel | null>(
		null,
	);
	const [assetToRemove, setAssetToRemove] = useState<IAssetViewModel | null>(
		null,
	);
	const [assetToReserve, setAssetToReserve] =
		useState<IAssetViewModel | null>(null);
	const [reservationIdToChange, setReservationIdToChange] = useState<
		string | null
	>(null);
	const [assetIdToFreeUp, setAssetIdToFreeUp] = useState<string | null>(null);

	const editAssetId = assetToUpdate?.id ?? null;
	const editAssetQuery = useAsset(editAssetId);
	const editCategoryAssetQuery = useCategoryAssetByAsset(editAssetId);
	const editLocationAssetQuery = useLocationAssetByAsset(editAssetId);
	const editOwnerAssetQuery = useOwnerAssetByAsset(editAssetId);
	const editQueries = [
		editAssetQuery,
		editCategoryAssetQuery,
		editLocationAssetQuery,
		editOwnerAssetQuery,
	];

	// `isSuccess`, not `!isLoading`: a failed query is also "not loading", and
	// opening the dialog then would hand it undefined data. Note the three
	// mapping queries resolve to `null` when an asset has no mapping yet —
	// still a success.
	const editDataReady =
		!!editAssetId && editQueries.every((query) => query.isSuccess);

	const changeReservationQuery = useAssetReservation(reservationIdToChange);
	const assetToChangeReservation =
		assetsReservedByUser.find(
			(asset) => asset.reservationId === reservationIdToChange,
		) ?? null;

	const assetToFreeUp =
		assetsReservedByUser.find((asset) => asset.id === assetIdToFreeUp) ??
		null;
	const freeUpReservationQuery = useAssetReservation(
		assetToFreeUp?.reservationId ?? null,
	);

	// RemoveReservationDialog renders the asset and user names alongside the
	// reservation, which the reservation endpoint doesn't return — join them
	// from the row and the current identity.
	const reservationToFreeUp: IAssetReservationWithNames | null = useMemo(() => {
		if (!freeUpReservationQuery.data || !assetToFreeUp) return null;
		return {
			...freeUpReservationQuery.data,
			assetName: assetToFreeUp.assetName,
			userName: accountInfo?.name ?? "",
		};
	}, [freeUpReservationQuery.data, assetToFreeUp, accountInfo?.name]);

	// ------------------------------------------------------------- mutations

	const queryClient = useQueryClient();

	// Creating or editing an asset writes its category/location/owner mappings
	// too, so those lists have to go stale alongside the asset lists.
	const invalidateAssetData = () => {
		for (const queryKey of [
			qk.overviewRoot(),
			qk.assetsRoot(),
			qk.removedAssets(),
			qk.categoryAssets(),
			qk.locationAssets(),
			qk.ownerAssets(),
		]) {
			queryClient.invalidateQueries({ queryKey });
		}
	};

	const invalidateReservationData = () => {
		for (const queryKey of [qk.overviewRoot(), qk.assetReservations()]) {
			queryClient.invalidateQueries({ queryKey });
		}
	};

	const createAsset = useMutation({
		mutationFn: (data: IAssetViewModelCreate) =>
			unwrap(overviewService.createAsset(data)),
		onSuccess: invalidateAssetData,
	});

	const updateAsset = useMutation({
		mutationFn: (vars: { assetId: string; data: IAssetViewModelUpdate }) =>
			unwrap(overviewService.updateAsset(vars.assetId, vars.data)),
		onSuccess: invalidateAssetData,
	});

	const removeAsset = useMutation({
		mutationFn: (vars: { assetId: string; data: IAssetViewModelRemove }) =>
			unwrap(overviewService.removeAsset(vars.assetId, vars.data)),
		// Removing an asset also cancels its reservations.
		onSuccess: () => {
			invalidateAssetData();
			invalidateReservationData();
		},
	});

	const reserveAsset = useMutation({
		mutationFn: (vars: { assetId: string; data: IAssetReservationAdd }) =>
			unwrap(overviewService.reserveAsset(vars.assetId, vars.data)),
		onSuccess: invalidateReservationData,
	});

	const returnAsset = useMutation({
		mutationFn: (assetId: string) =>
			unwrap(overviewService.returnAsset(assetId)),
		onSuccess: invalidateReservationData,
	});

	const removeReservation = useMutation({
		mutationFn: (assetId: string) =>
			unwrap(overviewService.removeReservation(assetId)),
		onSuccess: invalidateReservationData,
	});

	const changeReservationTime = useMutation({
		mutationFn: (vars: {
			reservationId: string;
			data: IAssetReservationUpdate;
		}) =>
			unwrap(
				overviewService.changeReservationTime(
					vars.reservationId,
					vars.data,
				),
			),
		// The list key no longer prefix-matches per-id entries, so the edited
		// reservation's own entry must be invalidated explicitly — otherwise
		// reopening the dialog within staleTime would show the pre-edit times.
		onSuccess: (_data, vars) => {
			invalidateReservationData();
			queryClient.invalidateQueries({
				queryKey: qk.assetReservation(vars.reservationId),
			});
		},
	});

	// ---------------------------------------------------------------- search

	const submitSearch = useCallback(
		(value: string) => {
			const params = new URLSearchParams();
			if (value) params.set("searchTerm", value);
			router.push(`${window.location.pathname}?${params.toString()}`);
		},
		[router],
	);

	const handleSearch = (e: React.SubmitEvent) => {
		e.preventDefault();
		submitSearch(searchInput);
	};

	// Mirrors the dialogs' `open` props rather than the selection state: a
	// selection whose data failed to load never opens a dialog, and must not
	// leave the scanner disabled with no way to re-enable it.
	const anyDialogOpen =
		showCreateModal ||
		editDataReady ||
		!!assetToRemove ||
		!!assetToReserve ||
		(changeReservationQuery.isSuccess && !!assetToChangeReservation) ||
		!!reservationToFreeUp;

	useBarcodeScanner({
		onScan: useCallback(
			(code: string) => {
				setSearchInput(code);
				submitSearch(code);
			},
			[submitSearch],
		),
		enabled: !anyDialogOpen,
	});

	const setQueryParam = useCallback(
		(key: string, value: string | null) => {
			const params = new URLSearchParams(searchParams.toString());
			if (value === null) {
				params.delete(key);
			} else {
				params.set(key, value);
			}
			router.push(`${window.location.pathname}?${params.toString()}`, {
				scroll: false,
			});
		},
		[router, searchParams],
	);

	// A refetch can drop the selected reservation's row (returned or removed
	// from another tab, say). The dialog can't render without the row, so the
	// selection must be cleared — left set, it would keep `anyDialogOpen` true
	// and pin the barcode scanner off with nothing visible to close.
	useEffect(() => {
		if (
			reservationIdToChange &&
			overview.isSuccess &&
			!assetToChangeReservation
		) {
			setReservationIdToChange(null);
			setQueryParam("changeReservationId", null);
		}
	}, [
		reservationIdToChange,
		overview.isSuccess,
		assetToChangeReservation,
		setQueryParam,
	]);

	// -------------------------------------------------------------- handlers
	//
	// mutateAsync (not mutate) so a failure rejects here and can be reported
	// the way each dialog expects: an alert for the fire-and-forget actions, a
	// returned error object for the two dialogs that render it inline.

	const handleCreate = async (createAssetModel: IAssetViewModelCreate) => {
		try {
			await createAsset.mutateAsync(createAssetModel);
		} catch (error) {
			alert((error as Error).message || "Failed to create asset");
		} finally {
			setShowCreateModal(false);
		}
	};

	const handleEdit = async (
		assetId: string,
		updateAssetModel: IAssetViewModelUpdate,
	) => {
		try {
			await updateAsset.mutateAsync({ assetId, data: updateAssetModel });
		} catch (error) {
			alert((error as Error).message || "Failed to edit asset");
		} finally {
			setAssetToUpdate(null);
			setQueryParam("editId", null);
		}
	};

	const handleRemove = async (
		assetId: string,
		assetRemoveVm: IAssetViewModelRemove,
	) => {
		try {
			await removeAsset.mutateAsync({ assetId, data: assetRemoveVm });
		} catch (error) {
			alert((error as Error).message || "Failed to remove asset");
		} finally {
			setAssetToRemove(null);
			setQueryParam("removeId", null);
		}
	};

	// ReserveAssetDialog closes itself once this resolves without `success:
	// false`, so the error path must not throw.
	const handleReserve = async (
		assetId: string,
		assetReservationVm: IAssetReservationAdd,
	) => {
		try {
			await reserveAsset.mutateAsync({
				assetId,
				data: assetReservationVm,
			});
			return { success: true };
		} catch (error) {
			return {
				success: false,
				error: (error as Error).message || "Failed to reserve asset",
			};
		}
	};

	const handleReturnAsset = async (assetId: string) => {
		try {
			await returnAsset.mutateAsync(assetId);
		} catch (error) {
			alert((error as Error).message || "Failed to return asset");
		}
	};

	// RemoveReservationDialog closes itself unless this throws — so it doesn't.
	const handleRemoveReservation = async (assetId: string) => {
		try {
			await removeReservation.mutateAsync(assetId);
		} catch (error) {
			alert((error as Error).message || "Failed to remove reservation");
		} finally {
			setAssetIdToFreeUp(null);
		}
	};

	const handleChangeReservationTime = async (
		assetReservationId: string,
		updateData: IAssetReservationUpdate,
	): Promise<{ error?: string } | void> => {
		try {
			await changeReservationTime.mutateAsync({
				reservationId: assetReservationId,
				data: updateData,
			});
			setReservationIdToChange(null);
			setQueryParam("changeReservationId", null);
		} catch (error) {
			return {
				error:
					(error as Error).message ||
					"Failed to update reservation time",
			};
		}
	};

	// --------------------------------------------------------- render state

	// AssetList disables a row's actions while anything concerning that asset
	// is in flight — either its dialog data loading, or its mutation running.
	const loading: Record<string, boolean> = {};
	const markLoading = (id: string | null | undefined) => {
		if (id) loading[id] = true;
	};

	// `isLoading` (in flight), not `!isSuccess`: a query that errored is also
	// not successful, and keying off that would pin the spinner on forever.
	// A disabled query reports `isLoading: false`, so an asset with no
	// reservation to load never spins either. The `isError && isFetching`
	// term covers the retry after a failed load (re-clicking the row action
	// refetches): that refetch keeps `status: "error"`, so `isLoading` alone
	// would leave the row spinner off while the retry is in flight.
	const dialogDataLoading = (query: {
		isLoading: boolean;
		isError: boolean;
		isFetching: boolean;
	}) => query.isLoading || (query.isError && query.isFetching);

	const editDataLoading = editQueries.some(dialogDataLoading);

	if (editDataLoading) markLoading(editAssetId);
	if (dialogDataLoading(changeReservationQuery)) {
		markLoading(assetToChangeReservation?.id);
	}
	if (dialogDataLoading(freeUpReservationQuery)) markLoading(assetIdToFreeUp);
	if (updateAsset.isPending) markLoading(updateAsset.variables?.assetId);
	if (removeAsset.isPending) markLoading(removeAsset.variables?.assetId);
	if (reserveAsset.isPending) markLoading(reserveAsset.variables?.assetId);
	if (returnAsset.isPending) markLoading(returnAsset.variables);
	if (removeReservation.isPending) markLoading(removeReservation.variables);
	if (changeReservationTime.isPending) {
		markLoading(changeReservationTime.variables?.data.assetId);
	}

	const loadFailed =
		overview.isError ||
		// Without these the create/edit dropdowns would just render empty.
		categoriesQuery.isError ||
		locationsQuery.isError ||
		ownersQuery.isError ||
		editQueries.some((query) => query.isError) ||
		changeReservationQuery.isError ||
		freeUpReservationQuery.isError;

	// True only before the very first result: `keepPreviousData` keeps the old
	// list on screen while a new search term loads.
	if (overview.isLoading) {
		return <Spinner className="h-64" />;
	}

	return (
		<div className="min-h-screen bg-[#efefef] -mx-3 sm:-mx-4 px-6 sm:px-14 py-8 text-left">
			<h1 className="text-3xl font-bold text-[#424242] mb-6">
				{tAssetViewModel("AssetsOverview")}
			</h1>
			{loadFailed && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
				</div>
			)}
			<AssetList
				availableAssets={availableAssets}
				assetsReservedByUser={assetsReservedByUser}
				onEditAsset={async (asset: IAssetViewModel) => {
					// Re-clicking the same asset is the retry gesture after a
					// failed load: the query keys don't change, so an errored
					// query would otherwise never refetch and the row would be
					// stuck. A different asset changes the keys and fetches on
					// its own.
					if (asset.id === editAssetId) {
						for (const query of editQueries) {
							if (query.isError) query.refetch();
						}
					}
					setQueryParam("editId", asset.id);
					setAssetToUpdate(asset);
				}}
				onRemoveAsset={async (asset: IAssetViewModel) => {
					setQueryParam("removeId", asset.id);
					setAssetToRemove(asset);
				}}
				onReserveAsset={async (asset: IAssetViewModel) => {
					setQueryParam("reserveId", asset.id);
					setAssetToReserve(asset);
				}}
				loading={loading}
				mode={selectedMode}
				onModeChange={(m) => setSelectedMode(m)}
				searchInput={searchInput}
				onSearchChange={(v) => setSearchInput(v)}
				onSearchSubmit={handleSearch}
				createLoading={
					categoriesQuery.isLoading ||
					locationsQuery.isLoading ||
					ownersQuery.isLoading
				}
				onCreateAsset={async () => setShowCreateModal(true)}
				onChangeReservationTime={async (reservationId: string) => {
					// Same retry-on-re-click as onEditAsset.
					if (
						reservationId === reservationIdToChange &&
						changeReservationQuery.isError
					) {
						changeReservationQuery.refetch();
					}
					setQueryParam("changeReservationId", reservationId);
					setReservationIdToChange(reservationId);
				}}
				onReturnAsset={handleReturnAsset}
				onRemoveReservation={async (assetId: string) => {
					// Same retry-on-re-click as onEditAsset.
					if (
						assetId === assetIdToFreeUp &&
						freeUpReservationQuery.isError
					) {
						freeUpReservationQuery.refetch();
					}
					setAssetIdToFreeUp(assetId);
				}}
			/>
			<CreateAssetDialog
				open={showCreateModal}
				onClose={() => setShowCreateModal(false)}
				onSubmit={handleCreate}
				categories={categories}
				locations={locations}
				owners={owners}
				isLoading={createAsset.isPending}
			/>
			<EditAssetDialog
				open={editDataReady}
				asset={assetToUpdate}
				comment={editAssetQuery.data?.comment ?? ""}
				categoryAssets={editCategoryAssetQuery.data ?? null}
				ownerAssets={editOwnerAssetQuery.data ?? null}
				locationAssets={editLocationAssetQuery.data ?? null}
				categories={categories}
				owners={owners}
				locations={locations}
				onClose={() => {
					setAssetToUpdate(null);
					setQueryParam("editId", null);
				}}
				onConfirm={handleEdit}
				isLoading={updateAsset.isPending}
			/>
			<RemoveAssetDialog
				open={!!assetToRemove}
				asset={assetToRemove}
				onClose={() => {
					setAssetToRemove(null);
					setQueryParam("removeId", null);
				}}
				onConfirm={handleRemove}
				isLoading={removeAsset.isPending}
			/>
			<ReserveAssetDialog
				open={!!assetToReserve}
				asset={assetToReserve}
				onClose={() => {
					setAssetToReserve(null);
					setQueryParam("reserveId", null);
				}}
				onConfirm={handleReserve}
				isLoading={reserveAsset.isPending}
			/>
			<ChangeReservationTimeDialog
				open={
					changeReservationQuery.isSuccess &&
					!!assetToChangeReservation
				}
				assetReservationId={reservationIdToChange}
				asset={assetToChangeReservation}
				initialFrom={
					changeReservationQuery.data
						? new Date(changeReservationQuery.data.reservationFrom)
						: null
				}
				initialTo={
					changeReservationQuery.data
						? new Date(changeReservationQuery.data.reservationTo)
						: null
				}
				onClose={() => {
					setReservationIdToChange(null);
					setQueryParam("changeReservationId", null);
				}}
				onConfirm={handleChangeReservationTime}
				isLoading={changeReservationTime.isPending}
			/>
			<RemoveReservationDialog
				open={!!reservationToFreeUp}
				reservation={reservationToFreeUp}
				onClose={() => setAssetIdToFreeUp(null)}
				onConfirm={handleRemoveReservation}
				isLoading={removeReservation.isPending}
			/>
		</div>
	);
}
