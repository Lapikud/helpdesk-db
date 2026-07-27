"use client";

import { useTranslation } from "react-i18next";
import { AccountContext } from "@/context/AccountContext";
import { roomService } from "@/services";
import { useRouter } from "next/navigation";
import { useContext, useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRooms } from "@/hooks/queries/entityQueries";
import { qk } from "@/lib/queryKeys";
import { IRoom, IRoomAdd } from "@/types/domain/DomainTypes";
import { unwrap } from "@/services/errors";
import ListPageWrapper from "@/components/ListPageWrapper";
import DataTable from "@/components/DataTable";
import {
	ActionCell,
	EditButton,
	DeleteButton,
} from "@/components/TableActions";
import { CreateRoomDialog } from "@/components/dialogs/roomDialogs/CreateRoomDialog";
import { EditRoomDialog } from "@/components/dialogs/roomDialogs/EditRoomDialog";
import { DeleteRoomDialog } from "@/components/dialogs/roomDialogs/DeleteRoomDialog";

export default function Rooms() {
	const { t: tRoom } = useTranslation("room");
	const { t: tCommon } = useTranslation("common");

	const { accountInfo } = useContext(AccountContext);
	const router = useRouter();

	const isAdmin = accountInfo?.roles?.includes("admins");
	const isHelpdeskDbAdmin = accountInfo?.roles?.includes("helpdesk_db_admins");
	const canManage = isAdmin || isHelpdeskDbAdmin;

	// Admin-only page: AuthGuard covers authentication, but the role check is
	// this page's own.
	useEffect(() => {
		if (accountInfo && !canManage) router.push("/");
	}, [accountInfo, canManage, router]);

	const queryClient = useQueryClient();
	const { data = [], isError, error } = useRooms();

	const invalidate = () =>
		queryClient.invalidateQueries({ queryKey: qk.rooms() });

	const createRoom = useMutation({
		mutationFn: (dto: IRoomAdd) => unwrap(roomService.addAsync(dto)),
		onSuccess: invalidate,
	});
	const editRoom = useMutation({
		mutationFn: (dto: IRoom) => unwrap(roomService.updateAsync(dto)),
		onSuccess: invalidate,
	});
	const deleteRoom = useMutation({
		mutationFn: (id: string) => unwrap(roomService.deleteAsync(id)),
		onSuccess: invalidate,
	});

	const [showCreate, setShowCreate] = useState(false);
	const [showEdit, setShowEdit] = useState(false);
	const [showDelete, setShowDelete] = useState(false);

	const [roomToEdit, setRoomToEdit] = useState<IRoom | null>(null);
	const [roomToDelete, setRoomToDelete] = useState<IRoom | null>(null);

	// mutateAsync (not mutate) so a failure rejects and can be surfaced through
	// the dialogs' ConfirmResult contract.
	const handleCreate = async (dto: IRoomAdd) => {
		try {
			await createRoom.mutateAsync(dto);
			setShowCreate(false);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleEdit = async (dto: IRoom) => {
		try {
			await editRoom.mutateAsync(dto);
			setShowEdit(false);
			setRoomToEdit(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const handleDelete = async (id: string) => {
		try {
			await deleteRoom.mutateAsync(id);
			setShowDelete(false);
			setRoomToDelete(null);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	const columns = canManage
		? [tRoom("RoomName"), tCommon("Comment"), tCommon("Actions")]
		: [tRoom("RoomName"), tCommon("Comment")];

	const rows = data.map((item) => ({
		id: item.id,
		cells: [
			item.roomName,
			item.comment || "-",
			...(canManage
				? [
						<ActionCell key="actions">
							<EditButton
								label={tCommon("EditLink")}
								onClick={() => {
									setRoomToEdit(item);
									setShowEdit(true);
								}}
							/>
							<DeleteButton
								label={tCommon("DeleteLink")}
								onClick={() => {
									setRoomToDelete(item);
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
			title={tRoom("Rooms")}
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

			<CreateRoomDialog
				open={showCreate}
				onClose={() => setShowCreate(false)}
				onConfirm={handleCreate}
				isLoading={createRoom.isPending}
			/>

			<EditRoomDialog
				open={showEdit}
				room={roomToEdit}
				onClose={() => {
					setShowEdit(false);
					setRoomToEdit(null);
				}}
				onConfirm={handleEdit}
				isLoading={editRoom.isPending}
			/>

			<DeleteRoomDialog
				open={showDelete}
				room={roomToDelete}
				onClose={() => {
					setShowDelete(false);
					setRoomToDelete(null);
				}}
				onConfirm={handleDelete}
				isLoading={deleteRoom.isPending}
			/>
		</ListPageWrapper>
	);
}
