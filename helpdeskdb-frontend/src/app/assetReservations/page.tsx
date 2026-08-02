"use client";

import { useTranslation } from "react-i18next";
import { assetReservationService } from "@/services";
import { useMemo } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import {
	useAssetReservations,
	useAssets,
	useRemovedAssets,
	useUsers,
} from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import {
	IAssetReservation,
	IAssetReservationAdd,
	IAssetReservationWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import ErrorBanner from "@/components/ErrorBanner";
import CreateButton from "@/components/CreateButton";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/CreateAssetReservationDialog";
import { EditAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/EditAssetReservationDialog";
import { DeleteAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/DeleteAssetReservationDialog";

export default function AssetReservations() {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");

	const {
		userId,
		canManage,
		canSeeReservationActions: showActions,
	} = usePermissions();

	// Four independent queries instead of one sequential fetch: each list is
	// cached under its own key, so the three lookup lists are shared with every
	// other page that needs them rather than refetched here.
	const { data: reservations, error } = useAssetReservations();
	const { data: assetsForNames } = useAssets(true);
	const { data: users } = useUsers();
	const { data: removedAssets } = useRemovedAssets();

	const crud = useEntityCrud<
		IAssetReservationWithNames,
		IAssetReservationAdd,
		IAssetReservation
	>({
		service: assetReservationService,
		// The overview's asset cards carry reservation state (`reserved`,
		// `reservationTo`, `closestReservationBy`), so a reservation mutation
		// here has to mark that list stale too — otherwise navigating to
		// /overview within its 30s staleTime serves the pre-mutation cache.
		invalidateKeys: [qk.assetReservationsRoot(), qk.overviewRoot()],
	});

	// Note the different cache key: the create dialog offers only non-removed
	// assets, so this is qk.assets(false) — a genuinely different list from the
	// qk.assets(true) used for name lookup above. Fetched only once the dialog
	// is opened, matching the previous lazy `loadAssets`.
	const { data: selectableAssets = [] } = useAssets(false, {
		enabled: crud.showCreate,
	});

	const data: IAssetReservationWithNames[] = useMemo(() => {
		if (!reservations) return [];

		const assetById = new Map((assetsForNames ?? []).map((a) => [a.id, a]));
		const userById = new Map((users ?? []).map((u) => [u.id, u]));
		const removedAssetIds = new Set(
			(removedAssets ?? []).map((ra) => ra.assetId),
		);

		return reservations
			.map((ar) => ({
				...ar,
				assetName:
					assetById.get(ar.assetId)?.assetName ?? "Unknown Asset",
				userName: userById.get(ar.userId)?.username ?? "Unknown User",
				isRemoved: removedAssetIds.has(ar.assetId),
			}))
			.sort(
				(a, b) =>
					new Date(b.reservationTo).getTime() -
					new Date(a.reservationTo).getTime(),
			);
	}, [reservations, assetsForNames, users, removedAssets]);

	const renderActionCell = (item: IAssetReservationWithNames) => {
		if (item.isRemoved) {
			return (
				<span className="text-sm font-medium text-red-600">
					{tAssetReservation("Removed")}
				</span>
			);
		}
		if (new Date(item.reservationTo) < new Date()) {
			return (
				<span className="text-sm font-medium text-[#c50000]">
					{tAssetReservation("Expired")}
				</span>
			);
		}
		if (item.userId === userId) {
			return (
				<>
					<EditButton
						label={tCommon("EditLink")}
						onClick={() => crud.openEdit(item)}
					/>
					<DeleteButton
						label={tCommon("DeleteLink")}
						onClick={() => crud.openDelete(item)}
					/>
				</>
			);
		}
		return null;
	};

	const columns = [
		tAssetReservation("AssetId"),
		tAssetReservation("UserId"),
		tAssetReservation("ReservationFrom"),
		tAssetReservation("ReservationTo"),
		tAssetReservation("IsReturned"),
		...(showActions ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.userName,
			new Date(item.reservationFrom).toLocaleString(),
			new Date(item.reservationTo).toLocaleString(),
			item.isReturned
				? tAssetReservation("Yes")
				: tAssetReservation("No"),
			...(showActions
				? [
						<ActionCell key="actions">
							{renderActionCell(item)}
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tAssetReservation("AssetReservations")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} minWidth="min-w-[700px]" />

			<CreateAssetReservationDialog
				assets={selectableAssets}
				{...crud.createDialogProps}
			/>

			{/* These three dialogs take `reservation`, not the generic `entity`
			    prop, so their props are wired explicitly rather than spread.
			    Phase 4 rebuilds them on the generic dialogs. */}
			<EditAssetReservationDialog
				open={crud.entityToEdit !== null}
				reservation={crud.entityToEdit}
				onClose={crud.editDialogProps.onClose}
				onConfirm={crud.handleEdit}
				isLoading={crud.editMutation.isPending}
			/>

			<DeleteAssetReservationDialog
				open={crud.entityToDelete !== null}
				reservation={crud.entityToDelete}
				onClose={crud.deleteDialogProps.onClose}
				onConfirm={crud.handleDelete}
				isLoading={crud.deleteMutation.isPending}
			/>
		</ListPageWrapper>
	);
}
