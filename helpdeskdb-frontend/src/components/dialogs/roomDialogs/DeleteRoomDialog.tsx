import { IRoom } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { roomDeleteSummary } from "../entityConfigs/room";

interface DeleteRoomDialogProps {
	open: boolean;
	room: IRoom | null;
	onClose: () => void;
	onConfirm: (roomId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteRoomDialog = ({
	open,
	room,
	onClose,
	onConfirm,
	isLoading,
}: DeleteRoomDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={room}
		namespace="room"
		singularKey="RoomSingular"
		summaryFields={roomDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
