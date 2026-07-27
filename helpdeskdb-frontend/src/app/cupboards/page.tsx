"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { cupboardService } from "@/services";
import { useContext, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useCupboards } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { ICupboard, ICupboardAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateCupboardDialog } from "@/components/dialogs/cupboardDialogs/CreateCupboardDialog";
import { EditCupboardDialog } from "@/components/dialogs/cupboardDialogs/EditCupboardDialog";
import { DeleteCupboardDialog } from "@/components/dialogs/cupboardDialogs/DeleteCupboardDialog";

export default function Cupboards() {
	const { t: tCupboard } = useTranslation("cupboard");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo } = useContext(AccountContext);
	const isAdmin = accountInfo?.roles?.includes("admins");
	const isHelpdeskDbAdmin = accountInfo?.roles?.includes("helpdesk_db_admins");

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useCupboards();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.cupboards() });

	const createCupboard = useMutation({
		mutationFn: (dto: ICupboardAdd) => unwrap(cupboardService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editCupboard = useMutation({
		mutationFn: (dto: ICupboard) => unwrap(cupboardService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteCupboard = useMutation({
		mutationFn: (id: string) => unwrap(cupboardService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [cupboardToEdit, setCupboardToEdit] = useState<ICupboard | null>(
		null,
	);
	const [cupboardToDelete, setCupboardToDelete] = useState<ICupboard | null>(
		null,
	);

	const handleCreate = async (dto: ICupboardAdd) => {
		try {
			await createCupboard.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ICupboard) => {
		try {
			await editCupboard.mutateAsync(dto);
			setShowEdit(false);
			setCupboardToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteCupboard.mutateAsync(id);
			setShowDelete(false);
			setCupboardToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = isAdmin || isHelpdeskDbAdmin
		? [tCupboard("CodeName"), tCommon("Actions")]
		: [tCupboard("CodeName")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.codeName,
			...(isAdmin || isHelpdeskDbAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setCupboardToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setCupboardToDelete(item);
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
			title={tCupboard("Cupboards")}
			createButton={
				(isAdmin || isHelpdeskDbAdmin) && (
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

			<CreateCupboardDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createCupboard.isPending}
			/>

			<EditCupboardDialog
				open={showEdit}
				cupboard={cupboardToEdit}
				onClose={() => {
					setShowEdit(false);
					setCupboardToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editCupboard.isPending}
			/>

			<DeleteCupboardDialog
				open={showDelete}
				cupboard={cupboardToDelete}
				onClose={() => {
					setShowDelete(false);
					setCupboardToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteCupboard.isPending}
			/>
		</ListPageWrapper>
	);
}
