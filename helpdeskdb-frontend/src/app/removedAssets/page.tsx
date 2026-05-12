"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RemovedAssetsService } from "@/services/RemovedAssetsService";
import { AssetService } from "@/services/AssetService";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import {
	IAsset,
	IRemovedAsset,
	IRemovedAssetAdd,
	IRemovedAssetWithAssetName,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateRemovedAssetDialog } from "@/components/dialogs/removedAssetsDialogs/CreateRemovedAssetDialog";
import { EditRemovedAssetDialog } from "@/components/dialogs/removedAssetsDialogs/EditRemovedAssetDialog";
import { DeleteRemovedAssetDialog } from "@/components/dialogs/removedAssetsDialogs/DeleteRemovedAssetDialog";

export default function RemovedAssets() {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const removedAssetsService = useMemo(() => new RemovedAssetsService(), []);
	const assetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		removedAssetsService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<IRemovedAssetWithAssetName[]>([]);
	const [assets, setAssets] = useState<IAsset[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [removedAssetToEdit, setRemovedAssetToEdit] =
		useState<IRemovedAssetWithAssetName | null>(null);
	const [removedAssetToDelete, setRemovedAssetToDelete] =
		useState<IRemovedAssetWithAssetName | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		try {
			const result = await removedAssetsService.getAllAsync();
			if (result.errors || !result.data) return;
			const assetsWithNames = await Promise.all(
				result.data.map(async (asset) => ({
					...asset,
					assetName: await assetService.getAssetNameById(asset.assetId),
				})),
			);
			setData(assetsWithNames);
		} catch (error) {
			console.error("Error fetching data:", error);
		}
	}, [assetService, removedAssetsService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const loadAssets = useCallback(async () => {
		if (assets.length > 0) return;
		const res = await assetService.getAllAsync();
		if (res.data) setAssets(res.data);
	}, [assets.length, assetService]);

	const handleCreate = async (dto: IRemovedAssetAdd) => {
		setCreateLoading(true);
		try {
			const result = await removedAssetsService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create removed asset",
				};
			}
			await fetchData();
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateLoading(false);
		}
	};

	const handleEdit = async (dto: IRemovedAsset) => {
		setEditLoading(true);
		try {
			const result = await removedAssetsService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to update removed asset",
				};
			}
			await fetchData();
			setShowEdit(false);
			setRemovedAssetToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await removedAssetsService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to delete removed asset",
				};
			}
			await fetchData();
			setShowDelete(false);
			setRemovedAssetToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tRemovedAssets("Asset"),
				tCommon("Comment"),
				tRemovedAssets("RemovedBy"),
				tCommon("Actions"),
			]
		: [
				tRemovedAssets("Asset"),
				tCommon("Comment"),
				tRemovedAssets("RemovedBy"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.comment || "-",
			item.removedBy,
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={async () => {
									await loadAssets();
									setRemovedAssetToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setRemovedAssetToDelete(item);
									setShowDelete(true);
								}}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tRemovedAssets("RemovedAssetsTitle")}
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
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<CreateRemovedAssetDialog
				open={showCreate}
				assets={assets}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditRemovedAssetDialog
				open={showEdit}
				removedAsset={removedAssetToEdit}
				assets={assets}
				onClose={() => {
					setShowEdit(false);
					setRemovedAssetToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteRemovedAssetDialog
				open={showDelete}
				removedAsset={removedAssetToDelete}
				onClose={() => {
					setShowDelete(false);
					setRemovedAssetToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
