"use client";

import { useTranslation } from "react-i18next";
import { categoryAssetsService } from "@/services";
import { useMemo } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import {
	useAssets,
	useCategories,
	useCategoryAssets,
} from "@/hooks/queries/entityQueries";
import { useEntityCrud } from "@/hooks/useEntityCrud";
import { qk } from "@/lib/queryKeys";
import {
	ICategoryAsset,
	ICategoryAssetAdd,
	ICategoryAssetWithNames,
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
	categoriesToOptions,
	categoryAssetCreateConfig,
	categoryAssetDeleteSummary,
	categoryAssetEditConfig,
	categoryAssetToAdd,
	categoryAssetToForm,
	categoryAssetToUpdate,
} from "@/components/dialogs/entityConfigs/categoryAsset";

export default function CategoryAssets() {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const { data: categoryAssets, error } = useCategoryAssets();
	const { data: assets } = useAssets(true);
	const { data: categories } = useCategories();

	const crud = useEntityCrud<
		ICategoryAssetWithNames,
		ICategoryAssetAdd,
		ICategoryAsset
	>({
		service: categoryAssetsService,
		invalidateKeys: [qk.categoryAssets()],
		decorateCreate: (dto) => ({ ...dto, createdBy: userName ?? "" }),
	});

	const data: ICategoryAssetWithNames[] = useMemo(() => {
		if (!categoryAssets) return [];

		const assetById = new Map((assets ?? []).map((a) => [a.id, a]));
		const categoryById = new Map((categories ?? []).map((c) => [c.id, c]));

		return categoryAssets.map((ca) => ({
			...ca,
			assetName: assetById.get(ca.assetId)?.assetName ?? ca.assetId,
			categoryName:
				categoryById.get(ca.categoryId)?.categoryName ?? ca.categoryId,
		}));
	}, [categoryAssets, assets, categories]);

	// The create dialog only offers assets that aren't mapped to a category yet.
	const unusedAssets = useMemo(() => {
		const used = new Set((categoryAssets ?? []).map((ca) => ca.assetId));
		return (assets ?? []).filter((a) => !used.has(a.id));
	}, [assets, categoryAssets]);

	const createOptions = useMemo(
		() => ({
			assets: assetsToOptions(unusedAssets),
			categories: categoriesToOptions(categories ?? []),
		}),
		[unusedAssets, categories],
	);

	// The edit dialog renders the asset as a display field, so only the
	// category list is selectable.
	const editOptions = useMemo(
		() => ({ categories: categoriesToOptions(categories ?? []) }),
		[categories],
	);

	const columns = [
		tCategoryAssets("Asset"),
		tCategoryAssets("Category"),
		tCommon("CreatedBy"),
		...(canManage ? [tCommon("Actions")] : []),
	];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.assetName,
			item.categoryName,
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
			title={tCategoryAssets("CategoryAssetsTitle")}
			createButton={canManage && <CreateButton onClick={crud.openCreate} />}
		>
			<ErrorBanner error={error} />
			<DataTable columns={columns} rows={rows} />

			<EntityFormDialog
				mode="create"
				config={categoryAssetCreateConfig}
				options={createOptions}
				{...crud.createDialogProps}
				onConfirm={(form) => crud.handleCreate(categoryAssetToAdd(form))}
			/>

			<EntityEditDialog
				config={categoryAssetEditConfig}
				toForm={categoryAssetToForm}
				toUpdate={categoryAssetToUpdate}
				options={editOptions}
				staticValues={{ assetName: crud.entityToEdit?.assetName ?? "" }}
				{...crud.editDialogProps}
			/>

			<EntityDeleteDialog
				namespace={categoryAssetCreateConfig.namespace}
				singularKey={categoryAssetCreateConfig.singularKey}
				summaryFields={categoryAssetDeleteSummary}
				{...crud.deleteDialogProps}
			/>
		</ListPageWrapper>
	);
}
