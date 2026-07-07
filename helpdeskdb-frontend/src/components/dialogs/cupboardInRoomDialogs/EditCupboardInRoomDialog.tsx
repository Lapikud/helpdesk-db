import { useMemo } from "react";
import {
	ICupboardInRoom,
	ICupboardInRoomWithNames,
	IRoom,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	cupboardInRoomEditConfig,
	roomsToOptions,
} from "../entityConfigs/cupboardInRoom";

interface EditCupboardInRoomDialogProps {
	open: boolean;
	cupboardInRoom: ICupboardInRoomWithNames | null;
	rooms: IRoom[];
	onClose: () => void;
	onConfirm: (data: ICupboardInRoom) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditCupboardInRoomDialog = ({
	open,
	cupboardInRoom,
	rooms,
	onClose,
	onConfirm,
	isLoading,
}: EditCupboardInRoomDialogProps) => {
	const options = useMemo(() => ({ rooms: roomsToOptions(rooms) }), [rooms]);
	const initialValues = useMemo(
		() =>
			cupboardInRoom
				? {
						roomId: cupboardInRoom.roomId,
						comment: cupboardInRoom.comment,
				  }
				: null,
		[cupboardInRoom],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={cupboardInRoomEditConfig}
			initialValues={initialValues}
			options={options}
			staticValues={{ codeName: cupboardInRoom?.codeName ?? "" }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					id: cupboardInRoom!.id,
					cupboardId: cupboardInRoom!.cupboardId,
					roomId: data.roomId,
					comment: data.comment,
				})
			}
			isLoading={isLoading}
		/>
	);
};
