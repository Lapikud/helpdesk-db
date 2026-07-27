"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { categoryService } from "@/services";
import { useContext, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCategories } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { ICategory, ICategoryAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
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

	const { accountInfo } = useContext(AccountContext);
	const isAdmin = accountInfo?.roles?.includes("admins");
	const isHelpdeskDbAdmin = accountInfo?.roles?.includes("helpdesk_db_admins");
	const canManage = isAdmin || isHelpdeskDbAdmin;

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useCategories();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.categories() });

	const createCategory = useMutation({
		mutationFn: (dto: ICategoryAdd) => unwrap(categoryService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editCategory = useMutation({
		mutationFn: (dto: ICategory) => unwrap(categoryService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteCategory = useMutation({
		mutationFn: (id: string) => unwrap(categoryService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [categoryToEdit, setCategoryToEdit] = useState<ICategory | null>(
		null,
	);
	const [categoryToDelete, setCategoryToDelete] = useState<ICategory | null>(
		null,
	);

	// mutateAsync (not mutate) so a failure rejects and can be surfaced through
	// the dialogs' ConfirmResult contract.
	const handleCreate = async (dto: ICategoryAdd) => {
		try {
			await createCategory.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ICategory) => {
		try {
			await editCategory.mutateAsync(dto);
			setShowEdit(false);
			setCategoryToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteCategory.mutateAsync(id);
			setShowDelete(false);
			setCategoryToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [tCategory("CategoryName"), tCommon("Comment"), tCommon("Actions")]
		: [tCategory("CategoryName"), tCommon("Comment")];

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

			<CreateCategoryDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createCategory.isPending}
			/>

			<EditCategoryDialog
				open={showEdit}
				category={categoryToEdit}
				onClose={() => {
					setShowEdit(false);
					setCategoryToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editCategory.isPending}
			/>

			<DeleteCategoryDialog
				open={showDelete}
				category={categoryToDelete}
				onClose={() => {
					setShowDelete(false);
					setCategoryToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteCategory.isPending}
			/>
		</ListPageWrapper>
	);
}
