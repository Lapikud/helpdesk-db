import { IRoomAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { roomFormConfig } from "../entityConfigs/room";

interface CreateRoomDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IRoomAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateRoomDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateRoomDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={roomFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
