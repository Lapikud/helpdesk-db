import { useMemo } from "react";
import { IRoom } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { roomFormConfig, roomToForm } from "../entityConfigs/room";

interface EditRoomDialogProps {
	open: boolean;
	room: IRoom | null;
	onClose: () => void;
	onConfirm: (data: IRoom) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditRoomDialog = ({
	open,
	room,
	onClose,
	onConfirm,
	isLoading,
}: EditRoomDialogProps) => {
	const initialValues = useMemo(
		() => (room ? roomToForm(room) : null),
		[room],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={roomFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: room!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
