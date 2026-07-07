import { useMemo } from "react";
import {
	ICupboard,
	ICupboardInRoomAdd,
	IRoom,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	cupboardInRoomCreateConfig,
	cupboardsToOptions,
	roomsToOptions,
} from "../entityConfigs/cupboardInRoom";

interface CreateCupboardInRoomDialogProps {
	open: boolean;
	cupboards: ICupboard[];
	rooms: IRoom[];
	onClose: () => void;
	onConfirm: (data: ICupboardInRoomAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateCupboardInRoomDialog = ({
	open,
	cupboards,
	rooms,
	onClose,
	onConfirm,
	isLoading,
}: CreateCupboardInRoomDialogProps) => {
	const options = useMemo(
		() => ({
			cupboards: cupboardsToOptions(cupboards),
			rooms: roomsToOptions(rooms),
		}),
		[cupboards, rooms],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={cupboardInRoomCreateConfig}
			options={options}
			onClose={onClose}
			onConfirm={onConfirm}
			isLoading={isLoading}
		/>
	);
};
