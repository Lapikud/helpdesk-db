"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CupboardService } from "@/services/CupboardService";
import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { ICupboard, ICupboardAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
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

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[],
	);
	if (setAccountInfo) cupboardService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<ICupboard[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [cupboardToEdit, setCupboardToEdit] = useState<ICupboard | null>(
		null,
	);
	const [cupboardToDelete, setCupboardToDelete] = useState<ICupboard | null>(
		null,
	);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await cupboardService.getAllAsync();
		if (!result.errors && result.data) setData(result.data);
	}, [cupboardService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const handleCreate = async (dto: ICupboardAdd) => {
		setCreateLoading(true);
		try {
			const result = await cupboardService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create cupboard",
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

	const handleEdit = async (dto: ICupboard) => {
		setEditLoading(true);
		try {
			const result = await cupboardService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update cupboard",
				};
			}
			await fetchData();
			setShowEdit(false);
			setCupboardToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await cupboardService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete cupboard",
				};
			}
			await fetchData();
			setShowDelete(false);
			setCupboardToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [tCupboard("CodeName"), tCommon("Actions")]
		: [tCupboard("CodeName")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.codeName,
			...(isAdmin
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

			<CreateCupboardDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditCupboardDialog
				open={showEdit}
				cupboard={cupboardToEdit}
				onClose={() => {
					setShowEdit(false);
					setCupboardToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteCupboardDialog
				open={showDelete}
				cupboard={cupboardToDelete}
				onClose={() => {
					setShowDelete(false);
					setCupboardToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
