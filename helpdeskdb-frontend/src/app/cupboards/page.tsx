"use client";

import { useTranslation } from "react-i18next";
import { cupboardService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useCupboards } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { ICupboard, ICupboardAdd } from "@/types/domain/DomainTypes";
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
	cupboardDeleteSummary,
	cupboardFormConfig,
	cupboardToForm,
} from "@/components/dialogs/entityConfigs/cupboard";

export default function Cupboards() {
	const { t: tCupboard } = useTranslation("cupboard");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useCupboards();
	const crud = useEntityCrud<ICupboard, ICupboardAdd>({
		service: cupboardService,
		invalidateKeys: [qk.cupboards()],
	});

	const columns = [
		tCupboard("CodeName"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.codeName,
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
			title={tCupboard("Cupboards")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={cupboardFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={cupboardFormConfig}
				toForm={cupboardToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={cupboardFormConfig.namespace}
				singularKey={cupboardFormConfig.singularKey}
				summaryFields={cupboardDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
