import { ICupboardInRoomWithNames } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { cupboardInRoomDeleteSummary } from "../entityConfigs/cupboardInRoom";

interface DeleteCupboardInRoomDialogProps {
	open: boolean;
	cupboardInRoom: ICupboardInRoomWithNames | null;
	onClose: () => void;
	onConfirm: (id: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteCupboardInRoomDialog = ({
	open,
	cupboardInRoom,
	onClose,
	onConfirm,
	isLoading,
}: DeleteCupboardInRoomDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={cupboardInRoom}
		namespace="cupboardinroom"
		singularKey="CupboardInRoomSingular"
		summaryFields={cupboardInRoomDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
