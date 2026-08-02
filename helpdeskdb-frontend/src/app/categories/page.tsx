"use client";

import { useTranslation } from "react-i18next";
import { categoryService } from "@/services";
import { usePermissions } from "@/hooks/usePermissions";
import { useCategories } from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import { ICategory, ICategoryAdd } from "@/types/domain/DomainTypes";
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
	categoryDeleteSummary,
	categoryFormConfig,
	categoryToForm,
} from "@/components/dialogs/entityConfigs/category";

export default function Categories() {
	const { t: tCategory } = useTranslation("category");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const { data = [], error } = useCategories();
	const crud = useEntityCrud<ICategory, ICategoryAdd>({
		service: categoryService,
		invalidateKeys: [qk.categories()],
	});

	const columns = [
		tCategory("CategoryName"),
		tCommon("Comment"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.categoryName,
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
			title={tCategory("Categories")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={categoryFormConfig}
				{...crud.createDialogProps}
			/>

			<EntityEditDialog
				config={categoryFormConfig}
				toForm={categoryToForm}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={categoryFormConfig.namespace}
				singularKey={categoryFormConfig.singularKey}
				summaryFields={categoryDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
