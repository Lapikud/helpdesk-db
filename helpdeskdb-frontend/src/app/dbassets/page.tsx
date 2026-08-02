"use client";

import { useTranslation } from "react-i18next";
import { assetService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useAssets } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { IAsset, IAssetAdd } from "@/types/domain/DomainTypes";
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
	dbAssetDeleteSummary,
	dbAssetFormConfig,
	dbAssetToAdd,
	dbAssetToForm,
	dbAssetToUpdate,
} from "@/components/dialogs/entityConfigs/dbAsset";

export default function Assets() {
	const { t: tAsset } = useTranslation("asset");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useAssets(true);
	// Root key so both includeRemoved variants refetch — the false variant
	// feeds the reservation create dialog's asset list.
	const crud = useEntityCrud<IAsset, IAssetAdd>({
		service: assetService,
		invalidateKeys: [qk.assetsRoot(), qk.overviewRoot()],
	});

	const columns = [
		tAsset("AssetName"),
		tAsset("SerialNumber"),
		tAsset("Barcode"),
		tCommon("Comment"),
		...(canManage ? [tCommon("Actions")] : []),
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
			title={tAsset("Assets")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={dbAssetFormConfig}
				{...crud.createDialogProps}
				onConfirm={(form) => crud.handleCreate(dbAssetToAdd(form))}
			/>

			<EntityEditDialog
				config={dbAssetFormConfig}
				toForm={dbAssetToForm}
				toUpdate={dbAssetToUpdate}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={dbAssetFormConfig.namespace}
				singularKey={dbAssetFormConfig.singularKey}
				summaryFields={dbAssetDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
