"use client";

import { useTranslation } from "react-i18next";
import { ownerService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useOwners } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { IOwner, IOwnerAdd } from "@/types/domain/DomainTypes";
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
	ownerDeleteSummary,
	ownerFormConfig,
	ownerToForm,
} from "@/components/dialogs/entityConfigs/owner";

export default function Owners() {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useOwners();
	const crud = useEntityCrud<IOwner, IOwnerAdd>({
		service: ownerService,
		invalidateKeys: [qk.owners()],
	});

	const columns = [
		tOwner("OwnerName"),
		tCommon("Comment"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.ownerName,
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
			title={tOwner("Owners")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={ownerFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={ownerFormConfig}
				toForm={ownerToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={ownerFormConfig.namespace}
				singularKey={ownerFormConfig.singularKey}
				summaryFields={ownerDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
