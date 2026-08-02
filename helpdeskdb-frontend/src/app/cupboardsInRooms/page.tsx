"use client";

import { useTranslation } from "react-i18next";
import { cupboardService, cupboardsInRoomsService } from "@/services";
import { useMemo, useState } from "react";
import { usePermissions } from "@/hooks/usePermissions";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
	useCupboards,
	useCupboardsInRooms,
	useRooms,
} from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { unwrap } from "@/services/errors";
import {
	ICupboardAdd,
	ICupboardInRoom,
	ICupboardInRoomAdd,
	ICupboardInRoomWithNames,
} from "@/types/domain/DomainTypes";
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

	const { canManage } = usePermissions();

	const {
		data: cupboardsInRooms,
		isError,
		error,
	} = useCupboardsInRooms();
	const { data: allCupboards } = useCupboards();
	const { data: rooms } = useRooms();

	const data: ICupboardInRoomWithNames[] = useMemo(() => {
		if (!cupboardsInRooms) return [];

		const cupboardMap = new Map(
			(allCupboards ?? []).map((c) => [c.id, c.codeName]),
		);
		const roomMap = new Map((rooms ?? []).map((r) => [r.id, r.roomName]));

		return cupboardsInRooms.map((r) => ({
			...r,
			roomName: roomMap.get(r.roomId) ?? r.roomId,
			codeName: cupboardMap.get(r.cupboardId) ?? r.cupboardId,
		}));
	}, [cupboardsInRooms, allCupboards, rooms]);

	// The create dialog only offers cupboards that aren't placed in a room yet.
	const availableCupboards = useMemo(() => {
		const used = new Set((cupboardsInRooms ?? []).map((d) => d.cupboardId));
		return (allCupboards ?? []).filter((c) => !used.has(c.id));
	}, [cupboardsInRooms, allCupboards]);

	const queryClient = useQueryClient();
	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.cupboardsInRooms() });

	const createCupboardInRoom = useMutation({
		mutationFn: (dto: ICupboardInRoomAdd) =>
			unwrap(cupboardsInRoomsService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const createCupboard = useMutation({
		mutationFn: (dto: ICupboardAdd) => unwrap(cupboardService.addAsync(dto)),
		onSuccess: () =>
			queryClient.invalidateQueries({ queryKey: qk.cupboards() }),
	});
	const editCupboardInRoom = useMutation({
		mutationFn: (dto: ICupboardInRoom) =>
			unwrap(cupboardsInRoomsService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteCupboardInRoom = useMutation({
		mutationFn: (id: string) =>
			unwrap(cupboardsInRoomsService.deleteAsync(id)),
		onSuccess: invalidate,
	});

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

	const handleCreate = async (dto: ICupboardInRoomAdd) => {
		try {
			await createCupboardInRoom.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleCreateCupboard = async (dto: ICupboardAdd) => {
		try {
			await createCupboard.mutateAsync(dto);
			setShowCreateCupboard(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: ICupboardInRoom) => {
		try {
			await editCupboardInRoom.mutateAsync(dto);
			setShowEdit(false);
			setCupboardInRoomToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteCupboardInRoom.mutateAsync(id);
			setShowDelete(false);
			setCupboardInRoomToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
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
			...(canManage
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
				canManage && (
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
			{isError && (
				<div className="mb-4 rounded-lg bg-red-100 border border-red-300 text-red-700 px-4 py-3">
					{tCommon("LoadFailed")}
					{error?.message ? `: ${error.message}` : ""}
				</div>
			)}
			<DataTable columns={columns} rows={rows} minWidth="min-w-[600px]" />

			<CreateCupboardDialog
				open={showCreateCupboard}
				onClose={() => setShowCreateCupboard(false)}
				onConfirm={handleCreateCupboard}
				isLoading={createCupboard.isPending}
			/>

			<CreateCupboardInRoomDialog
				open={showCreate}
				cupboards={availableCupboards}
				rooms={rooms ?? []}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createCupboardInRoom.isPending}
			/>

			<EditCupboardInRoomDialog
				open={showEdit}
				cupboardInRoom={cupboardInRoomToEdit}
				rooms={rooms ?? []}
				onClose={() => {
					setShowEdit(false);
					setCupboardInRoomToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editCupboardInRoom.isPending}
			/>

			<DeleteCupboardInRoomDialog
				open={showDelete}
				cupboardInRoom={cupboardInRoomToDelete}
				onClose={() => {
					setShowDelete(false);
					setCupboardInRoomToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteCupboardInRoom.isPending}
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
