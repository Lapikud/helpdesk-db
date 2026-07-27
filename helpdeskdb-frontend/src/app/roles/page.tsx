"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { roleService } from "@/services";
import { useRouter } from "next/navigation";
import { useContext, useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRoles } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { IRole, IRoleAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateRoleDialog } from "@/components/dialogs/roleDialogs/CreateRoleDialog";
import { EditRoleDialog } from "@/components/dialogs/roleDialogs/EditRoleDialog";
import { DeleteRoleDialog } from "@/components/dialogs/roleDialogs/DeleteRoleDialog";

export default function Roles() {
	const { t: tRole } = useTranslation("approle");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo } = useContext(AccountContext);
	const router = useRouter();

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isHelpdeskDbAdmin = accountInfo?.roles?.includes("helpdesk_db_admins");
	const canManage = isAdmin || isHelpdeskDbAdmin;

	useEffect(() => {
		if (accountInfo && !canManage) router.push("/");
	}, [accountInfo, canManage, router]);

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useRoles();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.roles() });

	const createRole = useMutation({
		mutationFn: (dto: IRoleAdd) => unwrap(roleService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editRole = useMutation({
		mutationFn: (dto: IRole) => unwrap(roleService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteRole = useMutation({
		mutationFn: (id: string) => unwrap(roleService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [roleToEdit, setRoleToEdit] = useState<IRole | null>(null);
	const [roleToDelete, setRoleToDelete] = useState<IRole | null>(null);

	const handleCreate = async (dto: IRoleAdd) => {
		try {
			await createRole.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IRole) => {
		try {
			await editRole.mutateAsync(dto);
			setShowEdit(false);
			setRoleToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteRole.mutateAsync(id);
			setShowDelete(false);
			setRoleToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [tRole("AppRoleName"), tCommon("Actions")]
		: [tRole("AppRoleName")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.name,
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setRoleToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setRoleToDelete(item);
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
			title={tRole("AppRoles")}
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

			<CreateRoleDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createRole.isPending}
			/>

			<EditRoleDialog
				open={showEdit}
				role={roleToEdit}
				onClose={() => {
					setShowEdit(false);
					setRoleToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editRole.isPending}
			/>

			<DeleteRoleDialog
				open={showDelete}
				role={roleToDelete}
				onClose={() => {
					setShowDelete(false);
					setRoleToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteRole.isPending}
			/>
		</ListPageWrapper>
	);
}
