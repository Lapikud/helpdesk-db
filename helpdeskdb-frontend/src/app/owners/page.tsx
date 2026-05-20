"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { OwnerService } from "@/services/OwnerService";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { IOwner, IOwnerAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
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

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const ownerService: OwnerService = useMemo(() => new OwnerService(), []);
	if (setAccountInfo) ownerService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<IOwner[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [ownerToEdit, setOwnerToEdit] = useState<IOwner | null>(null);
	const [ownerToDelete, setOwnerToDelete] = useState<IOwner | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await ownerService.getAllAsync();
		if (!result.errors && result.data) setData(result.data);
	}, [ownerService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const handleCreate = async (dto: IOwnerAdd) => {
		setCreateLoading(true);
		try {
			const result = await ownerService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create owner",
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

	const handleEdit = async (dto: IOwner) => {
		setEditLoading(true);
		try {
			const result = await ownerService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to update owner",
				};
			}
			await fetchData();
			setShowEdit(false);
			setOwnerToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await ownerService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to delete owner",
				};
			}
			await fetchData();
			setShowDelete(false);
			setOwnerToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tOwner("OwnerName"), tCommon("Comment"), tCommon("Actions")]
		: [tOwner("OwnerName"), tCommon("Comment")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.ownerName,
			item.comment || "-",
			...(isAdmin
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

			<CreateOwnerDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditOwnerDialog
				open={showEdit}
				owner={ownerToEdit}
				onClose={() => {
					setShowEdit(false);
					setOwnerToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteOwnerDialog
				open={showDelete}
				owner={ownerToDelete}
				onClose={() => {
					setShowDelete(false);
					setOwnerToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
