import { ICupboard } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { cupboardDeleteSummary } from "../entityConfigs/cupboard";

interface DeleteCupboardDialogProps {
	open: boolean;
	cupboard: ICupboard | null;
	onClose: () => void;
	onConfirm: (cupboardId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteCupboardDialog = ({
	open,
	cupboard,
	onClose,
	onConfirm,
	isLoading,
}: DeleteCupboardDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={cupboard}
		namespace="cupboard"
		singularKey="CupboardSingular"
		summaryFields={cupboardDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
