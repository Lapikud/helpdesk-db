"use client";

import { useTranslation } from "react-i18next";
import { assetReservationService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useAssetReservations,
	useAssets,
	useRemovedAssets,
	useUsers,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	IAssetReservation,
	IAssetReservationAdd,
	IAssetReservationWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
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

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	// Four independent queries instead of one sequential fetch: each list is
	// cached under its own key, so the three lookup lists are shared with every
	// other page that needs them rather than refetched here.
	const {
		data: reservations,
		isError,
		error,
	} = useAssetReservations();
	const { data: assetsForNames } = useAssets(true);
	const { data: users } = useUsers();
	const { data: removedAssets } = useRemovedAssets();

	// Note the different cache key: the create dialog offers only non-removed
	// assets, so this is qk.assets(false) — a genuinely different list from the
	// qk.assets(true) used for name lookup above. Fetched only once the dialog
	// is opened, matching the previous lazy `loadAssets`.
	const { data: selectableAssets = [] } = useAssets(false, {
		enabled: showCreate,
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

	const queryClient = useQueryClient();
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.assetReservationsRoot() });

	const createReservation = useMutation({
		mutationFn: (dto: IAssetReservationAdd) =>
			unwrap(assetReservationService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editReservation = useMutation({
		mutationFn: (dto: IAssetReservation) =>
			unwrap(assetReservationService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteReservation = useMutation({
		mutationFn: (id: string) =>
			unwrap(assetReservationService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [reservationToEdit, setReservationToEdit] =
		useState<IAssetReservationWithNames | null>(null);
	const [reservationToDelete, setReservationToDelete] =
		useState<IAssetReservationWithNames | null>(null);

	const handleCreate = async (dto: IAssetReservationAdd) => {
		try {
			await createReservation.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IAssetReservation) => {
		try {
			await editReservation.mutateAsync(dto);
			setShowEdit(false);
			setReservationToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteReservation.mutateAsync(id);
			setShowDelete(false);
			setReservationToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

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
						onClick={() => {
							setReservationToEdit(item);
							setShowEdit(true);
						}}
					/>
					<DeleteButton
						label={tCommon("DeleteLink")}
						onClick={() => {
							setReservationToDelete(item);
							setShowDelete(true);
						}}
					/>
				</>
			);
		}
		return null;
	};

	const columns = showActions
		? [
				tAssetReservation("AssetId"),
				tAssetReservation("UserId"),
				tAssetReservation("ReservationFrom"),
				tAssetReservation("ReservationTo"),
				tAssetReservation("IsReturned"),
				tCommon("Actions"),
			]
		: [
				tAssetReservation("AssetId"),
				tAssetReservation("UserId"),
				tAssetReservation("ReservationFrom"),
				tAssetReservation("ReservationTo"),
				tAssetReservation("IsReturned"),
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
			createButton={
				canManage && (
					<button
						type="button"
						onClick={() => setShowCreate(true)}
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</button>
				)
			}
		>
			{isError && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
					{error?.message ? `: ${error.message}` : ""}
				</div>
			)}
			<DataTable columns={columns} rows={rows} minWidth="min-w-[700px]" />

			<CreateAssetReservationDialog
				open={showCreate}
				assets={selectableAssets}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createReservation.isPending}
			/>

			<EditAssetReservationDialog
				open={showEdit}
				reservation={reservationToEdit}
				onClose={() => {
					setShowEdit(false);
					setReservationToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editReservation.isPending}
			/>

			<DeleteAssetReservationDialog
				open={showDelete}
				reservation={reservationToDelete}
				onClose={() => {
					setShowDelete(false);
					setReservationToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteReservation.isPending}
			/>
		</ListPageWrapper>
	);
}
