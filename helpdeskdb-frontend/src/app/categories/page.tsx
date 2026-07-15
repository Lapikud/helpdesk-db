"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CategoryService } from "@/services/CategoryService";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { ICategory, ICategoryAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateCategoryDialog } from "@/components/dialogs/categoryDialogs/CreateCategoryDialog";
import { EditCategoryDialog } from "@/components/dialogs/categoryDialogs/EditCategoryDialog";
import { DeleteCategoryDialog } from "@/components/dialogs/categoryDialogs/DeleteCategoryDialog";

export default function Categories() {
	const { t: tCategory } = useTranslation("category");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const categoryService: CategoryService = useMemo(
		() => new CategoryService(),
		[],
	);
	if (setAccountInfo) categoryService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<ICategory[]>([]);
	const [fetchError, setFetchError] = useState(false);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [categoryToEdit, setCategoryToEdit] = useState<ICategory | null>(
		null,
	);
	const [categoryToDelete, setCategoryToDelete] = useState<ICategory | null>(
		null,
	);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await categoryService.getAllAsync();
		if (!result.errors && result.data) {
			setData(result.data);
			setFetchError(false);
		} else {
			setFetchError(true);
		}
	}, [categoryService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const handleCreate = async (dto: ICategoryAdd) => {
		setCreateLoading(true);
		try {
			const result = await categoryService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create category",
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

	const handleEdit = async (dto: ICategory) => {
		setEditLoading(true);
		try {
			const result = await categoryService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update category",
				};
			}
			await fetchData();
			setShowEdit(false);
			setCategoryToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await categoryService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete category",
				};
			}
			await fetchData();
			setShowDelete(false);
			setCategoryToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tCategory("CategoryName"), tCommon("Comment"), tCommon("Actions")]
		: [tCategory("CategoryName"), tCommon("Comment")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.categoryName,
			item.comment || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setCategoryToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setCategoryToDelete(item);
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
			title={tCategory("Categories")}
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
			{fetchError && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
				</div>
			)}
			<DataTable columns={columns} rows={rows} />

			<CreateCategoryDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditCategoryDialog
				open={showEdit}
				category={categoryToEdit}
				onClose={() => {
					setShowEdit(false);
					setCategoryToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteCategoryDialog
				open={showDelete}
				category={categoryToDelete}
				onClose={() => {
					setShowDelete(false);
					setCategoryToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
