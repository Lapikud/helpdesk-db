
"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { LocationService } from "@/services/LocationService";
import {
	useCallback,
	useContext,
	useEffect,
	useMemo,
	useState,
} from "react";
import { ILocation, ILocationAdd } from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateLocationDialog } from "@/components/dialogs/locationDialogs/CreateLocationDialog";
import { EditLocationDialog } from "@/components/dialogs/locationDialogs/EditLocationDialog";
import { DeleteLocationDialog } from "@/components/dialogs/locationDialogs/DeleteLocationDialog";

export default function Locations() {
	const { t: tLocation } = useTranslation("location");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const locationService: LocationService = useMemo(
		() => new LocationService(),
		[],
	);
	if (setAccountInfo) locationService.injectSetAccountInfo(setAccountInfo);

	const [data, setData] = useState<ILocation[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [locationToEdit, setLocationToEdit] = useState<ILocation | null>(null);
	const [locationToDelete, setLocationToDelete] = useState<ILocation | null>(
		null,
	);

	const [createLoading, setCreateLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const result = await locationService.getAllAsync();
		if (!result.errors && result.data) setData(result.data);
	}, [locationService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const handleCreate = async (dto: ILocationAdd) => {
		setCreateLoading(true);
		try {
			const result = await locationService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create location",
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

	const handleEdit = async (dto: ILocation) => {
		setEditLoading(true);
		try {
			const result = await locationService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to update location",
				};
			}
			await fetchData();
			setShowEdit(false);
			setLocationToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await locationService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to delete location",
				};
			}
			await fetchData();
			setShowDelete(false);
			setLocationToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
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
			...(isAdmin
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
			<DataTable columns={columns} rows={rows} minWidth="min-w-[500px]" />

			<CreateLocationDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditLocationDialog
				open={showEdit}
				location={locationToEdit}
				onClose={() => {
					setShowEdit(false);
					setLocationToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteLocationDialog
				open={showDelete}
				location={locationToDelete}
				onClose={() => {
					setShowDelete(false);
					setLocationToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>
		</ListPageWrapper>
	);
}
