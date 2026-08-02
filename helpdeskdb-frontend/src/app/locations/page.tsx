"use client";

import { useTranslation } from "react-i18next";
import { locationService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useLocations } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { ILocation, ILocationAdd } from "@/types/domain/DomainTypes";
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
	locationDeleteSummary,
	locationFormConfig,
	locationToForm,
} from "@/components/dialogs/entityConfigs/location";

export default function Locations() {
	const { t: tLocation } = useTranslation("location");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useLocations();
	const crud = useEntityCrud<ILocation, ILocationAdd>({
		service: locationService,
		invalidateKeys: [qk.locations()],
	});

	const columns = [
		tLocation("LocationName"),
		tLocation("ShelfNum"),
		tLocation("Column"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.locationName,
			item.shelfNum,
			item.column,
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
			title={tLocation("Locations")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<EntityFormDialog
				mode="create"
				config={locationFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={locationFormConfig}
				toForm={locationToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={locationFormConfig.namespace}
				singularKey={locationFormConfig.singularKey}
				summaryFields={locationDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
