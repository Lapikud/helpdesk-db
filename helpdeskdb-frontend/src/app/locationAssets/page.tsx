"use client";

import { useTranslation } from "react-i18next";
import { locationAssetsService } from "@/services";
import { useMemo } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import {
	useAssets,
	useLocations,
	useLocationAssets,
} from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import {
	ILocationAsset,
	ILocationAssetAdd,
	ILocationAssetWithNames,
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
	locationAssetDeleteSummary,
	locationAssetFormConfig,
	locationAssetToAdd,
	locationAssetToForm,
	locationAssetToUpdate,
	locationsToOptions,
} from "@/components/dialogs/entityConfigs/locationAsset";

export default function LocationAssets() {
	const { t: tLocationAssets } = useTranslation("locationassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const { data: locationAssets, error } = useLocationAssets();
	const { data: assets } = useAssets(true);
	const { data: locations } = useLocations();

	const crud = useEntityCrud<
		ILocationAssetWithNames,
		ILocationAssetAdd,
		ILocationAsset
	>({
		service: locationAssetsService,
		invalidateKeys: [qk.locationAssets(), qk.overviewRoot()],
		decorateCreate: (dto) => ({ ...dto, createdBy: userName ?? "" }),
	});
	const { entityToEdit } = crud;

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

	// The edit dialog offers the unused assets plus the row's own asset — so
	// the mapping can keep its asset or move to a free one, but never steal an
	// asset already mapped to another location.
	const assetsForEdit = useMemo(() => {
		if (!entityToEdit) return unusedAssets;
		const current = (assets ?? []).find((a) => a.id === entityToEdit.assetId);
		if (!current || unusedAssets.some((a) => a.id === current.id)) {
			return unusedAssets;
		}
		return [current, ...unusedAssets];
	}, [unusedAssets, assets, entityToEdit]);

	const createOptions = useMemo(
		() => ({
			assets: assetsToOptions(unusedAssets),
			locations: locationsToOptions(locations ?? []),
		}),
		[unusedAssets, locations],
	);

	const editOptions = useMemo(
		() => ({
			assets: assetsToOptions(assetsForEdit),
			locations: locationsToOptions(locations ?? []),
		}),
		[assetsForEdit, locations],
	);

	const columns = [
		tLocationAssets("Asset"),
		tLocationAssets("Location"),
		tCommon("CreatedBy"),
		...(canManage ? [tCommon("Actions")] : []),
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
			title={tLocationAssets("LocationAssetsTitle")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<EntityFormDialog
				mode="create"
				config={locationAssetFormConfig}
				options={createOptions}
				{...crud.createDialogProps}
				onConfirm={(form) => crud.handleCreate(locationAssetToAdd(form))}
			/>

			<EntityEditDialog
				config={locationAssetFormConfig}
				toForm={locationAssetToForm}
				toUpdate={locationAssetToUpdate}
				options={editOptions}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={locationAssetFormConfig.namespace}
				singularKey={locationAssetFormConfig.singularKey}
				summaryFields={locationAssetDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
