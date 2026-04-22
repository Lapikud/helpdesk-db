"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { AssetReservationService } from "@/services/AssetReservationService";
import { AssetService } from "@/services/AssetService";
import { UserService } from "@/services/UserService";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import {
	IAsset,
	IAssetReservation,
	IAssetReservationAdd,
	IAssetReservationWithNames,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/CreateAssetReservationDialog";
import { EditAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/EditAssetReservationDialog";
import { DeleteAssetReservationDialog } from "@/components/dialogs/assetReservationDialogs/DeleteAssetReservationDialog";

export default function AssetReservations() {
	const { t: tAssetReservation } = useTranslation("assetreservation");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const assetReservationService = useMemo(() => new AssetReservationService(), []);
	const assetService = useMemo(() => new AssetService(), []);
	const userService = useMemo(() => new UserService(), []);
	const removedAssetsService = useMemo(() => new RemovedAssetsService(), []);

	if (setAccountInfo) {
		assetReservationService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
		userService.injectSetAccountInfo(setAccountInfo);
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<IAssetReservationWithNames[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const [assets, setAssets] = useState<IAsset[]>([]);

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [reservationToEdit, setReservationToEdit] =
		useState<IAssetReservationWithNames | null>(null);
	const [reservationToDelete, setReservationToDelete] =
		useState<IAssetReservationWithNames | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins") ?? false;
	const isMember = accountInfo?.roles?.includes("members") ?? false;
	const isPixel = accountInfo?.roles?.includes("pixels") ?? false;
	const showActions = isAdmin || isMember || isPixel;

	useEffect(() => { setHydrated(true); }, []);

	const fetchData = useCallback(async () => {
		try {
			const result = await assetReservationService.getAllAsync();
			if (result.errors) return;
			if (result.data) {
				const [assetsResult, usersResult, removedResult] = await Promise.all([
					assetService.getAllAsync(true),
					userService.getAllAsync(),
					removedAssetsService.getAllAsync(),
				]);
				const enrichedData: IAssetReservationWithNames[] = result.data.map((ar) => ({
					...ar,
					assetName: assetsResult.data?.find((a) => a.id === ar.assetId)?.assetName ?? "Unknown Asset",
					userName: usersResult.data?.find((u) => u.id === ar.userId)?.username ?? "Unknown User",
					isRemoved: removedResult.data?.some((ra) => ra.assetId === ar.assetId) ?? false,
				}));
				enrichedData.sort(
					(a, b) => new Date(b.reservationTo).getTime() - new Date(a.reservationTo).getTime(),
				);
				setData(enrichedData);
			}
		} catch (error) {
			console.error("Error fetching data:", error);
		}
	}, [assetReservationService, assetService, userService, removedAssetsService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const loadAssets = useCallback(async () => {
		if (assets.length > 0) return;
		const res = await assetService.getAllAsync();
		if (res.data) setAssets(res.data);
	}, [assets.length, assetService]);

	const handleCreate = async (dto: IAssetReservationAdd) => {
		setCreateLoading(true);
		try {
			const result = await assetReservationService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return { error: result.errors?.join(", ") || "Failed to create reservation" };
			}
			await fetchData();
			setShowCreate(false);
		} catch (error) {
			console.error("Error creating reservation:", error);
			return { error: (error as Error).message };
		} finally {
			setCreateLoading(false);
		}
	};

	const handleEdit = async (dto: IAssetReservation) => {
		setEditLoading(true);
		try {
			const result = await assetReservationService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return { error: result.errors?.join(", ") || "Failed to update reservation" };
			}
			await fetchData();
			setShowEdit(false);
			setReservationToEdit(null);
		} catch (error) {
			console.error("Error updating reservation:", error);
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await assetReservationService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return { error: result.errors?.join(", ") || "Failed to delete reservation" };
			}
			await fetchData();
			setShowDelete(false);
			setReservationToDelete(null);
		} catch (error) {
			console.error("Error deleting reservation:", error);
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

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
		if (item.userId === accountInfo?.id) {
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
			item.isReturned ? tAssetReservation("Yes") : tAssetReservation("No"),
			...(showActions
				? [<ActionCell key="actions">{renderActionCell(item)}</ActionCell>]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tAssetReservation("AssetReservations")}
			createButton={
				isAdmin && (
					<button
						type="button"
						onClick={async () => {
							await loadAssets();
							setShowCreate(true);
						}}
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</button>
				)
			}
		>
			<DataTable columns={columns} rows={rows} minWidth="min-w-[700px]" />

			<CreateAssetReservationDialog
				open={showCreate}
				assets={assets}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditAssetReservationDialog
				open={showEdit}
				reservation={reservationToEdit}
				onClose={() => {
					setShowEdit(false);
					setReservationToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteAssetReservationDialog
				open={showDelete}
				reservation={reservationToDelete}
				onClose={() => {
					setShowDelete(false);
					setReservationToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
