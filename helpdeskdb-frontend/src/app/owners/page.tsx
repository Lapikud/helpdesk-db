"use client";

import { useTranslation } from "react-i18next";
import { ownerService } from "@/services";
import { useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useOwners } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { IOwner, IOwnerAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateOwnerDialog } from "@/components/dialogs/ownerDialogs/CreateOwnerDialog";
import { EditOwnerDialog } from "@/components/dialogs/ownerDialogs/EditOwnerDialog";
import { DeleteOwnerDialog } from "@/components/dialogs/ownerDialogs/DeleteOwnerDialog";

export default function Owners() {
	const { t: tOwner } = useTranslation("owner");
	const { t: tCommon } = useTranslation("common");

	const { canManage } = usePermissions();

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useOwners();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.owners() });

	const createOwner = useMutation({
		mutationFn: (dto: IOwnerAdd) => unwrap(ownerService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editOwner = useMutation({
		mutationFn: (dto: IOwner) => unwrap(ownerService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteOwner = useMutation({
		mutationFn: (id: string) => unwrap(ownerService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [ownerToEdit, setOwnerToEdit] = useState<IOwner | null>(null);
	const [ownerToDelete, setOwnerToDelete] = useState<IOwner | null>(null);

	const handleCreate = async (dto: IOwnerAdd) => {
		try {
			await createOwner.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IOwner) => {
		try {
			await editOwner.mutateAsync(dto);
			setShowEdit(false);
			setOwnerToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteOwner.mutateAsync(id);
			setShowDelete(false);
			setOwnerToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [tOwner("OwnerName"), tCommon("Comment"), tCommon("Actions")]
		: [tOwner("OwnerName"), tCommon("Comment")];

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
								onClick={() => {
									setOwnerToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setOwnerToDelete(item);
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
			title={tOwner("Owners")}
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

			<CreateOwnerDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createOwner.isPending}
			/>

			<EditOwnerDialog
				open={showEdit}
				owner={ownerToEdit}
				onClose={() => {
					setShowEdit(false);
					setOwnerToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editOwner.isPending}
			/>

			<DeleteOwnerDialog
				open={showDelete}
				owner={ownerToDelete}
				onClose={() => {
					setShowDelete(false);
					setOwnerToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteOwner.isPending}
			/>
		</ListPageWrapper>
	);
}
