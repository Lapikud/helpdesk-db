"use client";

import { useTranslation } from "react-i18next";
import { removedAssetsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAssets, useRemovedAssets } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	IRemovedAsset,
	IRemovedAssetAdd,
	IRemovedAssetWithAssetName,
} from "@/types/domain/DomainTypes";
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

	const { canManage } = usePermissions();

	const { data: removedAssets, isError, error } = useRemovedAssets();
	// The join needs the includeRemoved variant — removed assets are absent
	// from the default list, which only feeds the dialog dropdowns below.
	const { data: allAssets } = useAssets(true);
	const { data: assets = [] } = useAssets(false, { enabled: !!canManage });

	const data: IRemovedAssetWithAssetName[] = useMemo(() => {
		if (!removedAssets) return [];

		const nameById = new Map(
			(allAssets ?? []).map((a) => [a.id, a.assetName]),
		);

		return removedAssets.map((ra) => ({
			...ra,
			assetName: nameById.get(ra.assetId) ?? ra.assetId,
		}));
	}, [removedAssets, allAssets]);

	const queryClient = useQueryClient();
	// Marking an asset removed (or undoing that) changes which assets the
	// includeRemoved=false lists contain, so both caches must refetch.
	const invalidate = () => {
		queryClient.invalidateQueries({ queryKey: qk.removedAssets() });
		queryClient.invalidateQueries({ queryKey: qk.assetsRoot() });
	};

	const createRemovedAsset = useMutation({
		mutationFn: (dto: IRemovedAssetAdd) =>
			unwrap(removedAssetsService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editRemovedAsset = useMutation({
		mutationFn: (dto: IRemovedAsset) =>
			unwrap(removedAssetsService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteRemovedAsset = useMutation({
		mutationFn: (id: string) =>
			unwrap(removedAssetsService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [removedAssetToEdit, setRemovedAssetToEdit] =
		useState<IRemovedAssetWithAssetName | null>(null);
	const [removedAssetToDelete, setRemovedAssetToDelete] =
		useState<IRemovedAssetWithAssetName | null>(null);

	// mutateAsync (not mutate) so a failure rejects and can be surfaced through
	// the dialogs' ConfirmResult contract.
	const handleCreate = async (dto: IRemovedAssetAdd) => {
		try {
			await createRemovedAsset.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IRemovedAsset) => {
		try {
			await editRemovedAsset.mutateAsync(dto);
			setShowEdit(false);
			setRemovedAssetToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteRemovedAsset.mutateAsync(id);
			setShowDelete(false);
			setRemovedAssetToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
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
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
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
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<CreateRemovedAssetDialog
				open={showCreate}
				assets={assets}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createRemovedAsset.isPending}
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
				isLoading={editRemovedAsset.isPending}
			/>

			<DeleteRemovedAssetDialog
				open={showDelete}
				removedAsset={removedAssetToDelete}
				onClose={() => {
					setShowDelete(false);
					setRemovedAssetToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteRemovedAsset.isPending}
			/>
		</ListPageWrapper>
	);
}
