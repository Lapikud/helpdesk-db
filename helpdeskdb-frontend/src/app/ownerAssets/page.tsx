"use client";

import { useTranslation } from "react-i18next";
import { ownerAssetsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useAssets,
	useOwners,
	useOwnerAssets,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	IOwnerAsset,
	IOwnerAssetAdd,
	IOwnerAssetWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/CreateOwnerAssetDialog";
import { EditOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/EditOwnerAssetDialog";
import { DeleteOwnerAssetDialog } from "@/components/dialogs/ownerAssetDialogs/DeleteOwnerAssetDialog";

export default function OwnerAssets() {
	const { t: tOwnerAssets } = useTranslation("ownerassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const {
		data: ownerAssets,
		isError,
		error,
	} = useOwnerAssets();
	const { data: assets } = useAssets(true);
	const { data: owners } = useOwners();

	const data: IOwnerAssetWithNames[] = useMemo(() => {
		if (!ownerAssets) return [];

		const assetById = new Map((assets ?? []).map((a) => [a.id, a]));
		const ownerById = new Map((owners ?? []).map((o) => [o.id, o]));

		return ownerAssets.map((oa) => ({
			...oa,
			assetName: assetById.get(oa.assetId)?.assetName ?? oa.assetId,
			ownerName:
				ownerById.get(oa.ownerId)?.ownerName ?? oa.ownerId,
		}));
	}, [ownerAssets, assets, owners]);

	// The create dialog only offers assets that aren't mapped to an owner yet.
	const unusedAssets = useMemo(() => {
		const used = new Set((ownerAssets ?? []).map((oa) => oa.assetId));
		return (assets ?? []).filter((a) => !used.has(a.id));
	}, [assets, ownerAssets]);

	const queryClient = useQueryClient();
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.ownerAssets() });

	const createOwnerAsset = useMutation({
		mutationFn: (dto: IOwnerAssetAdd) =>
			unwrap(ownerAssetsService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editOwnerAsset = useMutation({
		mutationFn: (dto: IOwnerAsset) =>
			unwrap(ownerAssetsService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteOwnerAsset = useMutation({
		mutationFn: (id: string) =>
			unwrap(ownerAssetsService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [itemToEdit, setItemToEdit] =
		useState<IOwnerAssetWithNames | null>(null);
	const [itemToDelete, setItemToDelete] =
		useState<IOwnerAssetWithNames | null>(null);

	const handleCreate = async (dto: IOwnerAssetAdd) => {
		try {
			await createOwnerAsset.mutateAsync({
				...dto,
				createdBy: userName ?? "",
			});
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IOwnerAsset) => {
		try {
			await editOwnerAsset.mutateAsync(dto);
			setShowEdit(false);
			setItemToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteOwnerAsset.mutateAsync(id);
			setShowDelete(false);
			setItemToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [
				tOwnerAssets("Asset"),
				tOwnerAssets("Owner"),
				tCommon("CreatedBy"),
				tCommon("Actions"),
			]
		: [tOwnerAssets("Asset"), tOwnerAssets("Owner"), tCommon("CreatedBy")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.ownerName,
			item.createdBy || "-",
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setItemToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setItemToDelete(item);
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
			title={tOwnerAssets("OwnerAssetsTitle")}
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

			<CreateOwnerAssetDialog
				open={showCreate}
				assets={unusedAssets}
				owners={owners ?? []}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createOwnerAsset.isPending}
			/>

			<EditOwnerAssetDialog
				open={showEdit}
				ownerAsset={itemToEdit}
				owners={owners ?? []}
				onClose={() => {
					setShowEdit(false);
					setItemToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editOwnerAsset.isPending}
			/>

			<DeleteOwnerAssetDialog
				open={showDelete}
				ownerAsset={itemToDelete}
				onClose={() => {
					setShowDelete(false);
					setItemToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteOwnerAsset.isPending}
			/>
		</ListPageWrapper>
	);
}
