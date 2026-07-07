"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { CupboardsInRoomsService } from "@/services/CupboardsInRoomsService";
import { CupboardService } from "@/services/CupboardService";
import { RoomService } from "@/services/RoomService";
import {
	useCallback,
	useContext,
	useEffect,
	useMemo,
	useState,
} from "react";
import {
	ICupboard,
	ICupboardAdd,
	ICupboardInRoom,
	ICupboardInRoomAdd,
	ICupboardInRoomWithNames,
	IRoom,
} from "@/types/domain/DomainTypes";
import Spinner from "@/components/LoadingSpinner";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import { ActionCell, EditButton, DeleteButton } from "@/components/TableActions";
import { CreateCupboardDialog } from "@/components/dialogs/cupboardDialogs/CreateCupboardDialog";
import { CreateCupboardInRoomDialog } from "@/components/dialogs/cupboardInRoomDialogs/CreateCupboardInRoomDialog";
import { EditCupboardInRoomDialog } from "@/components/dialogs/cupboardInRoomDialogs/EditCupboardInRoomDialog";
import { DeleteCupboardInRoomDialog } from "@/components/dialogs/cupboardInRoomDialogs/DeleteCupboardInRoomDialog";
import { CupboardLocationsDialog } from "@/components/dialogs/locationInCupboardDialogs/CupboardLocationsDialog";

export default function CupboardsInRooms() {
	const { t: tCupboardInRoom } = useTranslation("cupboardinroom");
	const { t: tCupboard } = useTranslation("cupboard");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo, setAccountInfo } = useContext(AccountContext);
	const cupboardsInRoomsService: CupboardsInRoomsService = useMemo(
		() => new CupboardsInRoomsService(),
		[],
	);
	const cupboardService: CupboardService = useMemo(
		() => new CupboardService(),
		[],
	);
	const roomService: RoomService = useMemo(() => new RoomService(), []);
	if (setAccountInfo) {
		cupboardsInRoomsService.injectSetAccountInfo(setAccountInfo);
		cupboardService.injectSetAccountInfo(setAccountInfo);
		roomService.injectSetAccountInfo(setAccountInfo);
	}

	const [data, setData] = useState<ICupboardInRoomWithNames[]>([]);
	const [allCupboards, setAllCupboards] = useState<ICupboard[]>([]);
	const [rooms, setRooms] = useState<IRoom[]>([]);
	const [hydrated, setHydrated] = useState(false);
	const isAdmin = accountInfo?.roles?.includes("admins");

	const [showCreate, setShowCreate] = useState(false);
	const [showCreateCupboard, setShowCreateCupboard] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);
	const [showLocations, setShowLocations] = useState(false);

	const [cupboardInRoomToEdit, setCupboardInRoomToEdit] =
		useState<ICupboardInRoomWithNames | null>(null);
	const [cupboardInRoomToDelete, setCupboardInRoomToDelete] =
		useState<ICupboardInRoomWithNames | null>(null);
	const [selectedCupboard, setSelectedCupboard] = useState<{
		id: string;
		codeName: string;
	} | null>(null);

	const [createLoading, setCreateLoading] = useState(false);
	const [createCupboardLoading, setCreateCupboardLoading] = useState(false);
	const [editLoading, setEditLoading] = useState(false);
	const [deleteLoading, setDeleteLoading] = useState(false);

	useEffect(() => {
		setHydrated(true);
	}, []);

	const fetchData = useCallback(async () => {
		const [cirResult, cupboardsResult, roomsResult] = await Promise.all([
			cupboardsInRoomsService.getAllAsync(),
			cupboardService.getAllAsync(),
			roomService.getAllAsync(),
		]);

		const cupboards = cupboardsResult.data ?? [];
		const roomList = roomsResult.data ?? [];
		setAllCupboards(cupboards);
		setRooms(roomList);

		const cupboardMap = new Map(cupboards.map((c) => [c.id, c.codeName]));
		const roomMap = new Map(roomList.map((r) => [r.id, r.roomName]));

		const records = cirResult.data ?? [];
		setData(
			records.map((r) => ({
				...r,
				roomName: roomMap.get(r.roomId) ?? r.roomId,
				codeName: cupboardMap.get(r.cupboardId) ?? r.cupboardId,
			})),
		);
	}, [cupboardsInRoomsService, cupboardService, roomService]);

	useEffect(() => {
		if (!hydrated) return;
		fetchData();
	}, [hydrated, fetchData]);

	const availableCupboards = useMemo(() => {
		const used = new Set(data.map((d) => d.cupboardId));
		return allCupboards.filter((c) => !used.has(c.id));
	}, [data, allCupboards]);

	const handleCreate = async (dto: ICupboardInRoomAdd) => {
		setCreateLoading(true);
		try {
			const result = await cupboardsInRoomsService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to create cupboard in room",
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

	const handleCreateCupboard = async (dto: ICupboardAdd) => {
		setCreateCupboardLoading(true);
		try {
			const result = await cupboardService.addAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") || "Failed to create cupboard",
				};
			}
			await fetchData();
			setShowCreateCupboard(false);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setCreateCupboardLoading(false);
		}
	};

	const handleEdit = async (dto: ICupboardInRoom) => {
		setEditLoading(true);
		try {
			const result = await cupboardsInRoomsService.updateAsync(dto);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to update cupboard in room",
				};
			}
			await fetchData();
			setShowEdit(false);
			setCupboardInRoomToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setEditLoading(false);
		}
	};

	const handleDelete = async (id: string) => {
		setDeleteLoading(true);
		try {
			const result = await cupboardsInRoomsService.deleteAsync(id);
			if (result.errors || (result.statusCode ?? 0) >= 400) {
				return {
					error:
						result.errors?.join(", ") ||
						"Failed to delete cupboard in room",
				};
			}
			await fetchData();
			setShowDelete(false);
			setCupboardInRoomToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		} finally {
			setDeleteLoading(false);
		}
	};

	if (!hydrated) return <Spinner className="h-64" />;

	const columns = isAdmin
		? [
				tCupboardInRoom("Room"),
				tCupboardInRoom("Cupboard"),
				tCommon("Comment"),
				tCommon("Actions"),
			]
		: [
				tCupboardInRoom("Room"),
				tCupboardInRoom("Cupboard"),
				tCommon("Comment"),
			];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.roomName,
			item.codeName,
			item.comment || "-",
			...(isAdmin
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setCupboardInRoomToEdit(item);
									setShowEdit(true);
								}}
							/>
							<button
								type="button"
								onClick={() => {
									setSelectedCupboard({
										id: item.cupboardId,
										codeName: item.codeName,
									});
									setShowLocations(true);
								}}
								className="text-sm font-medium py-2 px-4 rounded-xl whitespace-nowrap transition-colors duration-150 text-center block w-full bg-[#eeeeee] hover:bg-gray-200 text-[#616161]"
							>
								{tCommon("DetailsLink")}
							</button>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setCupboardInRoomToDelete(item);
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
			title={tCupboardInRoom("CupboardsInRooms")}
			createButton={
				isAdmin && (
					<div className="flex gap-2">
						<button
							type="button"
							onClick={() => setShowCreateCupboard(true)}
							className="border border-[#ff9800] text-[#f0941d] hover:bg-orange-50 font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
						>
							{tCupboard("NewCupboard")}
						</button>
						<button
							type="button"
							onClick={() => setShowCreate(true)}
							className="bg-[#ff9800] hover:bg-[#f0941d] text-white font-medium px-6 py-3 rounded-full text-sm whitespace-nowrap transition-colors duration-150"
						>
							{tCommon("CreateNewLink")}
						</button>
					</div>
				)
			}
		>
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<CreateCupboardDialog
				open={showCreateCupboard}
				onClose={() => setShowCreateCupboard(false)}
				onConfirm={handleCreateCupboard}
				isLoading={createCupboardLoading}
			/>

			<CreateCupboardInRoomDialog
				open={showCreate}
				cupboards={availableCupboards}
				rooms={rooms}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createLoading}
			/>

			<EditCupboardInRoomDialog
				open={showEdit}
				cupboardInRoom={cupboardInRoomToEdit}
				rooms={rooms}
				onClose={() => {
					setShowEdit(false);
					setCupboardInRoomToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editLoading}
			/>

			<DeleteCupboardInRoomDialog
				open={showDelete}
				cupboardInRoom={cupboardInRoomToDelete}
				onClose={() => {
					setShowDelete(false);
					setCupboardInRoomToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteLoading}
			/>

			<CupboardLocationsDialog
				open={showLocations}
				cupboard={selectedCupboard}
				onClose={() => {
					setShowLocations(false);
					setSelectedCupboard(null);
				}}
			/>
		</ListPageWrapper>
	);
}
