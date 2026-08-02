"use client";

import { useTranslation } from "react-i18next";
import { roleService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useRoles } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { IRole, IRoleAdd } from "@/types/domain/DomainTypes";
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
	roleDeleteSummary,
	roleFormConfig,
	roleToForm,
} from "@/components/dialogs/entityConfigs/role";

export default function Roles() {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useRoles();
	const crud = useEntityCrud<IRole, IRoleAdd>({
		service: roleService,
		invalidateKeys: [qk.roles()],
	});

	const columns = [
		tRole("AppRoleName"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.name,
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
			title={tRole("AppRoles")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={roleFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={roleFormConfig}
				toForm={roleToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={roleFormConfig.namespace}
				singularKey={roleFormConfig.singularKey}
				summaryFields={roleDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
