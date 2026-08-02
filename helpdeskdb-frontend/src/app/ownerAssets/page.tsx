"use client";

import { useTranslation } from "react-i18next";
import { ownerAssetsService } from "@/services";
import { useMemo } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import {
	useAssets,
	useOwners,
	useOwnerAssets,
} from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import {
	IOwnerAsset,
	IOwnerAssetAdd,
	IOwnerAssetWithNames,
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
	ownerAssetCreateConfig,
	ownerAssetDeleteSummary,
	ownerAssetEditConfig,
	ownerAssetToAdd,
	ownerAssetToForm,
	ownerAssetToUpdate,
	ownersToOptions,
} from "@/components/dialogs/entityConfigs/ownerAsset";

export default function OwnerAssets() {
	const { t: tOwnerAssets } = useTranslation("ownerassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const { data: ownerAssets, error } = useOwnerAssets();
	const { data: assets } = useAssets(true);
	const { data: owners } = useOwners();

	const crud = useEntityCrud<
		IOwnerAssetWithNames,
		IOwnerAssetAdd,
		IOwnerAsset
	>({
		service: ownerAssetsService,
		invalidateKeys: [qk.ownerAssets()],
		decorateCreate: (dto) => ({ ...dto, createdBy: userName ?? "" }),
	});

	const data: IOwnerAssetWithNames[] = useMemo(() => {
		if (!ownerAssets) return [];

		const assetById = new Map((assets ?? []).map((a) => [a.id, a]));
		const ownerById = new Map((owners ?? []).map((o) => [o.id, o]));

		return ownerAssets.map((oa) => ({
			...oa,
			assetName: assetById.get(oa.assetId)?.assetName ?? oa.assetId,
			ownerName: ownerById.get(oa.ownerId)?.ownerName ?? oa.ownerId,
		}));
	}, [ownerAssets, assets, owners]);

	// The create dialog only offers assets that aren't mapped to an owner yet.
	const unusedAssets = useMemo(() => {
		const used = new Set((ownerAssets ?? []).map((oa) => oa.assetId));
		return (assets ?? []).filter((a) => !used.has(a.id));
	}, [assets, ownerAssets]);

	const createOptions = useMemo(
		() => ({
			assets: assetsToOptions(unusedAssets),
			owners: ownersToOptions(owners ?? []),
		}),
		[unusedAssets, owners],
	);

	// The edit dialog renders the asset as a display field, so only the owner
	// list is selectable.
	const editOptions = useMemo(
		() => ({ owners: ownersToOptions(owners ?? []) }),
		[owners],
	);

	const columns = [
		tOwnerAssets("Asset"),
		tOwnerAssets("Owner"),
		tCommon("CreatedBy"),
		...(canManage ? [tCommon("Actions")] : []),
	];

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
			title={tOwnerAssets("OwnerAssetsTitle")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={ownerAssetCreateConfig}
				options={createOptions}
				{...crud.createDialogProps}
				onConfirm={(form) => crud.handleCreate(ownerAssetToAdd(form))}
			/>

			<EntityEditDialog
				config={ownerAssetEditConfig}
				toForm={ownerAssetToForm}
				toUpdate={ownerAssetToUpdate}
				options={editOptions}
				staticValues={{ assetName: crud.entityToEdit?.assetName ?? "" }}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={ownerAssetCreateConfig.namespace}
				singularKey={ownerAssetCreateConfig.singularKey}
				summaryFields={ownerAssetDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
