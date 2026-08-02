"use client";

import { useTranslation } from "react-i18next";
import { categoryAssetsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useAssets,
	useCategories,
	useCategoryAssets,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	ICategoryAsset,
	ICategoryAssetAdd,
	ICategoryAssetWithNames,
} from "@/types/domain/DomainTypes";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateCategoryAssetDialog } from "@/components/dialogs/categoryAssetDialogs/CreateCategoryAssetDialog";
import { EditCategoryAssetDialog } from "@/components/dialogs/categoryAssetDialogs/EditCategoryAssetDialog";
import { DeleteCategoryAssetDialog } from "@/components/dialogs/categoryAssetDialogs/DeleteCategoryAssetDialog";

export default function CategoryAssets() {
	const { t: tCategoryAssets } = useTranslation("categoryassets");
	const { t: tCommon } = useTranslation("common");

	const { canManage, userName } = usePermissions();

	const {
		data: categoryAssets,
		isError,
		error,
	} = useCategoryAssets();
	const { data: assets } = useAssets(true);
	const { data: categories } = useCategories();

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

	const queryClient = useQueryClient();
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.categoryAssets() });

	const createCategoryAsset = useMutation({
		mutationFn: (dto: ICategoryAssetAdd) =>
			unwrap(categoryAssetsService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editCategoryAsset = useMutation({
		mutationFn: (dto: ICategoryAsset) =>
			unwrap(categoryAssetsService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteCategoryAsset = useMutation({
		mutationFn: (id: string) =>
			unwrap(categoryAssetsService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [itemToEdit, setItemToEdit] =
		useState<ICategoryAssetWithNames | null>(null);
	const [itemToDelete, setItemToDelete] =
		useState<ICategoryAssetWithNames | null>(null);

	const handleCreate = async (dto: ICategoryAssetAdd) => {
		try {
			await createCategoryAsset.mutateAsync({
				...dto,
				createdBy: userName ?? "",
			});
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ICategoryAsset) => {
		try {
			await editCategoryAsset.mutateAsync(dto);
			setShowEdit(false);
			setItemToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteCategoryAsset.mutateAsync(id);
			setShowDelete(false);
			setItemToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [
				tCategoryAssets("Asset"),
				tCategoryAssets("Category"),
				tCommon("CreatedBy"),
				tCommon("Actions"),
			]
		: [
				tCategoryAssets("Asset"),
				tCategoryAssets("Category"),
				tCommon("CreatedBy"),
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
								onClick={() => {
									setItemToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setItemToDelete(item);
									setShowDelete(true);
								}}
							/>
						</ActionCell>,
					]
				: []),
		],
	}));

	return (
		<ListPageWrapper
			title={tCategoryAssets("CategoryAssetsTitle")}
			createButton={
				canManage && (
					<button
						type="button"
						onClick={() => setShowCreate(true)}
						className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
					>
						{tCommon("CreateNewLink")}
					</button>
				)
			}
		>
			{isError && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
					{error?.message ? `: ${error.message}` : ""}
				</div>
			)}
			<DataTable columns={columns} rows={rows} />

			<CreateCategoryAssetDialog
				open={showCreate}
				assets={unusedAssets}
				categories={categories ?? []}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createCategoryAsset.isPending}
			/>

			<EditCategoryAssetDialog
				open={showEdit}
				categoryAsset={itemToEdit}
				categories={categories ?? []}
				onClose={() => {
					setShowEdit(false);
					setItemToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editCategoryAsset.isPending}
			/>

			<DeleteCategoryAssetDialog
				open={showDelete}
				categoryAsset={itemToDelete}
				onClose={() => {
					setShowDelete(false);
					setItemToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteCategoryAsset.isPending}
			/>
		</ListPageWrapper>
	);
}
