"use client";

import { useTranslation } from "react-i18next";
import { assetService } from "@/services";
import { useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAssets } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { IAsset, IAssetAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
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

	const { canManage } = usePermissions();

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useAssets(true);

	// Root key so both includeRemoved variants refetch — the false variant
	// feeds the reservation create dialog's asset list.
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.assetsRoot() });

	const createAsset = useMutation({
		mutationFn: (dto: IAssetAdd) => unwrap(assetService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editAsset = useMutation({
		mutationFn: (dto: IAsset) => unwrap(assetService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteAsset = useMutation({
		mutationFn: (id: string) => unwrap(assetService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [assetToEdit, setAssetToEdit] = useState<IAsset | null>(null);
	const [assetToDelete, setAssetToDelete] = useState<IAsset | null>(null);


	const handleCreate = async (dto: IAssetAdd) => {
		try {
			await createAsset.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IAsset) => {
		try {
			await editAsset.mutateAsync(dto);
			setShowEdit(false);
			setAssetToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteAsset.mutateAsync(id);
			setShowDelete(false);
			setAssetToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
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
			...(canManage
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
			<DataTable columns={columns} rows={rows} />

			<CreateDbAssetDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createAsset.isPending}
			/>

			<EditDbAssetDialog
				open={showEdit}
				asset={assetToEdit}
				onClose={() => {
					setShowEdit(false);
					setAssetToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editAsset.isPending}
			/>

			<DeleteDbAssetDialog
				open={showDelete}
				asset={assetToDelete}
				onClose={() => {
					setShowDelete(false);
					setAssetToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteAsset.isPending}
			/>
		</ListPageWrapper>
	);
}
