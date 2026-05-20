"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { AssetService } from "@/services/AssetService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { IAsset, IAssetAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateDbAssetDialog } from "@/components/dialogs/dbAssetDialogs/CreateDbAssetDialog";
import { EditDbAssetDialog } from "@/components/dialogs/dbAssetDialogs/EditDbAssetDialog";
import { DeleteDbAssetDialog } from "@/components/dialogs/dbAssetDialogs/DeleteDbAssetDialog";

export default function Assets() {
	const { t: tAsset } = useTranslation("asset");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<IAsset[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [assetToEdit, setAssetToEdit] = useState<IAsset | null>(null);
	const [assetToDelete, setAssetToDelete] = useState<IAsset | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await assetService.getAllAsync(true);
		if (!result.errors && result.data) setData(result.data);
	}, [assetService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	const handleCreate = async (dto: IAssetAdd) => {
		setCreateLoading(true);
		try {
			const result = await assetService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create asset",
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

	const handleEdit = async (dto: IAsset) => {
		setEditLoading(true);
		try {
			const result = await assetService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to update asset",
				};
			}
			await fetchData();
			setShowEdit(false);
			setAssetToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await assetService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to delete asset",
				};
			}
			await fetchData();
			setShowDelete(false);
			setAssetToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tAsset("AssetName"),
				tAsset("SerialNumber"),
				tAsset("Barcode"),
				tCommon("Comment"),
				tCommon("Actions"),
			]
		: [
				tAsset("AssetName"),
				tAsset("SerialNumber"),
				tAsset("Barcode"),
				tCommon("Comment"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.serialNumber || "-",
			item.barcode || "-",
			item.comment || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setAssetToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setAssetToDelete(item);
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
			title={tAsset("Assets")}
			createButton={
				isAdmin && (
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
			<DataTable columns={columns} rows={rows} />

			<CreateDbAssetDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditDbAssetDialog
				open={showEdit}
				asset={assetToEdit}
				onClose={() => {
					setShowEdit(false);
					setAssetToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteDbAssetDialog
				open={showDelete}
				asset={assetToDelete}
				onClose={() => {
					setShowDelete(false);
					setAssetToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
