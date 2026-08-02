"use client";

import { useTranslation } from "react-i18next";
import { removedAssetsService } from "@/services";
import { useMemo } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useAssets, useRemovedAssets } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import {
	IAsset,
	IRemovedAsset,
	IRemovedAssetAdd,
	IRemovedAssetWithAssetName,
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
import { EntityFormDialog } from "@/components/dialogs/common/EntityFormDialog";
import { EntityEditDialog } from "@/components/dialogs/common/EntityEditDialog";
import { EntityDeleteDialog } from "@/components/dialogs/common/EntityDeleteDialog";
import {
	assetsToOptions,
	removedAssetCreateConfig,
	removedAssetDeleteSummary,
	removedAssetEditConfig,
	removedAssetToForm,
	removedAssetToUpdate,
} from "@/components/dialogs/entityConfigs/removedAsset";

export default function RemovedAssets() {
	const { t: tRemovedAssets } = useTranslation("removedassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const { data: removedAssets, error } = useRemovedAssets();
	// The join needs the includeRemoved variant — removed assets are absent
	// from the default list, which only feeds the dialog dropdowns below.
	const { data: allAssets } = useAssets(true);
	const { data: assets = [] } = useAssets(false, { enabled: !!canManage });

	// Marking an asset removed (or undoing that) changes which assets the
	// includeRemoved=false lists contain, so both caches must refetch.
	const crud = useEntityCrud<
		IRemovedAssetWithAssetName,
		IRemovedAssetAdd,
		IRemovedAsset
	>({
		service: removedAssetsService,
		invalidateKeys: [qk.removedAssets(), qk.assetsRoot(), qk.overviewRoot()],
	});

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

	const createOptions = useMemo(
		() => ({ assets: assetsToOptions(assets) }),
		[assets],
	);

	// The edited row's own asset is excluded from the "available assets" list
	// (it is removed), so append it back or the select can't show the current
	// value.
	const { entityToEdit } = crud;
	const editOptions = useMemo(() => {
		const withCurrent =
			!entityToEdit || assets.some((a) => a.id === entityToEdit.assetId)
				? assets
				: [
						...assets,
						{
							id: entityToEdit.assetId,
							assetName: entityToEdit.assetName,
						} as IAsset,
					];
		return { assets: assetsToOptions(withCurrent) };
	}, [assets, entityToEdit]);

	const columns = [
		tRemovedAssets("Asset"),
		tCommon("Comment"),
		tRemovedAssets("RemovedBy"),
		...(canManage ? [tCommon("Actions")] : []),
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
								onClick={() => crud.openEdit(item)}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => crud.openDelete(item)}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tRemovedAssets("RemovedAssetsTitle")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<EntityFormDialog
				mode="create"
				config={removedAssetCreateConfig}
				options={createOptions}
				staticValues={{ userName: userName ?? "" }}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={removedAssetEditConfig}
				toForm={removedAssetToForm}
				toUpdate={removedAssetToUpdate}
				options={editOptions}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={removedAssetCreateConfig.namespace}
				singularKey={removedAssetCreateConfig.singularKey}
				summaryFields={removedAssetDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
