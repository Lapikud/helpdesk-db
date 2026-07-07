"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { RoleService } from "@/services/RoleService";
import { useRouter } from "next/navigation";

import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { IRole, IRoleAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
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

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const roleService: RoleService = useMemo(() => new RoleService(), []);
	if (setAccountInfo) roleService.injectSetAccountInfo(setAccountInfo);

	const router = useRouter();
	const [data, setData] = useState<IRole[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [roleToEdit, setRoleToEdit] = useState<IRole | null>(null);
	const [roleToDelete, setRoleToDelete] = useState<IRole | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await roleService.getAllAsync();
		if (!result.errors && result.data) setData(result.data);
	}, [roleService]);

	useEffect(() => {
		if (!hydrated) return;

		if (!isAdmin) {
			router.push("/");
			return;
		}

		fetchData();
	}, [hydrated, router, isAdmin, fetchData]);

	const handleCreate = async (dto: IRoleAdd) => {
		setCreateLoading(true);
		try {
			const result = await roleService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create role",
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

	const handleEdit = async (dto: IRole) => {
		setEditLoading(true);
		try {
			const result = await roleService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to update role",
				};
			}
			await fetchData();
			setShowEdit(false);
			setRoleToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await roleService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to delete role",
				};
			}
			await fetchData();
			setShowDelete(false);
			setRoleToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tRole("AppRoleName"), tCommon("Actions")]
		: [tRole("AppRoleName")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.name,
			...(isAdmin
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

			<CreateRoleDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditRoleDialog
				open={showEdit}
				role={roleToEdit}
				onClose={() => {
					setShowEdit(false);
					setRoleToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteRoleDialog
				open={showDelete}
				role={roleToDelete}
				onClose={() => {
					setShowDelete(false);
					setRoleToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
