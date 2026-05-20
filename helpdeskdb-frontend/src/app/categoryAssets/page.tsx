"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CategoryAssetsService } from "@/services/CategoryAssetsService";
import { CategoryService } from "@/services/CategoryService";
import { AssetService } from "@/services/AssetService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import {
	IAsset,
	ICategory,
	ICategoryAssetAdd,
	ICategoryAssetWithNames,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
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

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const categoryAssetsService: CategoryAssetsService = useMemo(
		() => new CategoryAssetsService(),
		[],
	);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[],
	);
	const assetService: AssetService = useMemo(() => new AssetService(), []);
	if (setAccountInfo) {
		categoryAssetsService.injectSetAccountInfo(setAccountInfo);
		categoryService.injectSetAccountInfo(setAccountInfo);
		assetService.injectSetAccountInfo(setAccountInfo);
	}
	const router = useRouter();
	const [data, setData] = useState<ICategoryAssetWithNames[]>([]);
	const [allAssets, setAllAssets] = useState<IAsset[]>([]);
	const [allCategories, setAllCategories] = useState<ICategory[]>([]);
	const [hydrated, setHydrated] = useState(false);

	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [itemToEdit, setItemToEdit] =
		useState<ICategoryAssetWithNames | null>(null);
	const [itemToDelete, setItemToDelete] =
		useState<ICategoryAssetWithNames | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const [categoryAssetsResult, assetsResult, categoriesResult] =
			await Promise.all([
				categoryAssetsService.getAllAsync(),
				assetService.getAllAsync(true),
				categoryService.getAllAsync(),
			]);

		if (
			categoryAssetsResult.errors ||
			!categoryAssetsResult.data ||
			assetsResult.errors ||
			!assetsResult.data ||
			categoriesResult.errors ||
			!categoriesResult.data
		) {
			return;
		}

		const assetById = new Map(assetsResult.data.map((a) => [a.id, a]));
		const categoryById = new Map(
			categoriesResult.data.map((c) => [c.id, c]),
		);

		const withNames: ICategoryAssetWithNames[] =
			categoryAssetsResult.data.map((ca) => ({
				...ca,
				assetName: assetById.get(ca.assetId)?.assetName ?? ca.assetId,
				categoryName:
					categoryById.get(ca.categoryId)?.categoryName ??
					ca.categoryId,
			}));

		setData(withNames);
		setAllAssets(assetsResult.data);
		setAllCategories(categoriesResult.data);
	}, [categoryAssetsService, assetService, categoryService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	const unusedAssets = useMemo(() => {
		const used = new Set(data.map((ca) => ca.assetId));
		return allAssets.filter((a) => !used.has(a.id));
	}, [allAssets, data]);

	const handleCreate = async (dto: ICategoryAssetAdd) => {
		setCreateLoading(true);
		try {
			const result = await categoryAssetsService.addAsync({
				...dto,
				createdBy: accountInfo?.name ?? "",
			});
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create category asset",
				};
			}
			await fetchData();
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateLoading(false);
		}
	};

	const handleEdit = async (dto: ICategoryAssetWithNames) => {
		setEditLoading(true);
		try {
			const result = await categoryAssetsService.updateAsync({
				id: dto.id,
				assetId: dto.assetId,
				categoryId: dto.categoryId,
				comment: dto.comment,
				createdBy: accountInfo?.name ?? dto.createdBy,
			});
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update category asset",
				};
			}
			await fetchData();
			setShowEdit(false);
			setItemToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await categoryAssetsService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete category asset",
				};
			}
			await fetchData();
			setShowDelete(false);
			setItemToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
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
			...(isAdmin
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
				isAdmin && (
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
			<DataTable columns={columns} rows={rows} />

			<CreateCategoryAssetDialog
				open={showCreate}
				assets={unusedAssets}
				categories={allCategories}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditCategoryAssetDialog
				open={showEdit}
				categoryAsset={itemToEdit}
				categories={allCategories}
				onClose={() => {
					setShowEdit(false);
					setItemToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteCategoryAssetDialog
				open={showDelete}
				categoryAsset={itemToDelete}
				onClose={() => {
					setShowDelete(false);
					setItemToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
