"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { locationService } from "@/services";
import { useRouter } from "next/navigation";
import { useContext, useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useLocations } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { ILocation, ILocationAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateLocationDialog } from "@/components/dialogs/locationDialogs/CreateLocationDialog";
import { EditLocationDialog } from "@/components/dialogs/locationDialogs/EditLocationDialog";
import { DeleteLocationDialog } from "@/components/dialogs/locationDialogs/DeleteLocationDialog";

export default function Locations() {
	const { t: tLocation } = useTranslation("location");
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
	const { data = [], isError, error } = useLocations();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.locations() });
	
	const createLocation = useMutation({
		mutationFn: (dto: ILocationAdd) => unwrap(locationService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editLocation = useMutation({
		mutationFn: (dto: ILocation) => unwrap(locationService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteLocation = useMutation({
		mutationFn: (id: string) => unwrap(locationService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [locationToEdit, setLocationToEdit] = useState<ILocation | null>(null);
	const [locationToDelete, setLocationToDelete] = useState<ILocation | null>(
		null,
	);

	const handleCreate = async (dto: ILocationAdd) => {
		try {
			await createLocation.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ILocation) => {
		try {
			await editLocation.mutateAsync(dto);
			setShowEdit(false);
			setLocationToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteLocation.mutateAsync(id);
			setShowDelete(false);
			setLocationToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [
				tLocation("LocationName"),
				tLocation("ShelfNum"),
				tLocation("Column"),
				tCommon("Actions"),
			]
		: [
				tLocation("LocationName"),
				tLocation("ShelfNum"),
				tLocation("Column"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.locationName,
			item.shelfNum,
			item.column,
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setLocationToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setLocationToDelete(item);
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
			title={tLocation("Locations")}
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
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<CreateLocationDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLocation.isPending}
			/>

			<EditLocationDialog
				open={showEdit}
				location={locationToEdit}
				onClose={() => {
					setShowEdit(false);
					setLocationToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLocation.isPending}
			/>

			<DeleteLocationDialog
				open={showDelete}
				location={locationToDelete}
				onClose={() => {
					setShowDelete(false);
					setLocationToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLocation.isPending}
			/>
		</ListPageWrapper>
	);
}
