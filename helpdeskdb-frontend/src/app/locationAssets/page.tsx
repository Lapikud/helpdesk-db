"use client";

import { useTranslation } from "react-i18next";
import { locationAssetsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useAssets,
	useLocations,
	useLocationAssets,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	ILocationAsset,
	ILocationAssetAdd,
	ILocationAssetWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/CreateLocationAssetDialog";
import { EditLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/EditLocationAssetDialog";
import { DeleteLocationAssetDialog } from "@/components/dialogs/locationAssetDialogs/DeleteLocationAssetDialog";

export default function LocationAssets() {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const {
		data: locationAssets,
		isError,
		error,
	} = useLocationAssets();
	const { data: assets } = useAssets(true);
	const { data: locations } = useLocations();

	const data: ILocationAssetWithNames[] = useMemo(() => {
		if (!locationAssets) return [];

		const assetById = new Map((assets ?? []).map((a) => [a.id, a]));
		const locationById = new Map((locations ?? []).map((l) => [l.id, l]));

		return locationAssets.map((la) => ({
			...la,
			assetName: assetById.get(la.assetId)?.assetName ?? la.assetId,
			locationName:
				locationById.get(la.locationId)?.locationName ?? la.locationId,
		}));
	}, [locationAssets, assets, locations]);

	const unusedAssets = useMemo(() => {
		const used = new Set((locationAssets ?? []).map((la) => la.assetId));
		return (assets ?? []).filter((a) => !used.has(a.id));
	}, [assets, locationAssets]);

	const queryClient = useQueryClient();
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.locationAssets() });

	const createLocationAsset = useMutation({
		mutationFn: (dto: ILocationAssetAdd) =>
			unwrap(locationAssetsService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editLocationAsset = useMutation({
		mutationFn: (dto: ILocationAsset) =>
			unwrap(locationAssetsService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteLocationAsset = useMutation({
		mutationFn: (id: string) =>
			unwrap(locationAssetsService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [itemToEdit, setItemToEdit] =
		useState<ILocationAssetWithNames | null>(null);
	const [itemToDelete, setItemToDelete] =
		useState<ILocationAssetWithNames | null>(null);

	// The edit dialog offers the unused assets plus the row's own asset — so
	// the mapping can keep its asset or move to a free one, but never steal an
	// asset already mapped to another location.
	const assetsForEdit = useMemo(() => {
		if (!itemToEdit) return unusedAssets;
		const current = (assets ?? []).find((a) => a.id === itemToEdit.assetId);
		if (!current || unusedAssets.some((a) => a.id === current.id)) {
			return unusedAssets;
		}
		return [current, ...unusedAssets];
	}, [unusedAssets, assets, itemToEdit]);

	const handleCreate = async (dto: ILocationAssetAdd) => {
		try {
			await createLocationAsset.mutateAsync({
				...dto,
				createdBy: userName ?? "",
			});
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ILocationAsset) => {
		try {
			await editLocationAsset.mutateAsync(dto);
			setShowEdit(false);
			setItemToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteLocationAsset.mutateAsync(id);
			setShowDelete(false);
			setItemToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [
				tLocationAssets("Asset"),
				tLocationAssets("Location"),
				tCommon("CreatedBy"),
				tCommon("Actions"),
			]
		: [
				tLocationAssets("Asset"),
				tLocationAssets("Location"),
				tCommon("CreatedBy"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.locationName,
			item.createdBy,
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
			title={tLocationAssets("LocationAssetsTitle")}
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
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<CreateLocationAssetDialog
				open={showCreate}
				assets={unusedAssets}
				locations={locations ?? []}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLocationAsset.isPending}
			/>

			<EditLocationAssetDialog
				open={showEdit}
				locationAsset={itemToEdit}
				assets={assetsForEdit}
				locations={locations ?? []}
				onClose={() => {
					setShowEdit(false);
					setItemToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLocationAsset.isPending}
			/>

			<DeleteLocationAssetDialog
				open={showDelete}
				locationAsset={itemToDelete}
				onClose={() => {
					setShowDelete(false);
					setItemToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLocationAsset.isPending}
			/>
		</ListPageWrapper>
	);
}
