import { ICupboardAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { cupboardFormConfig } from "../entityConfigs/cupboard";

interface CreateCupboardDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: ICupboardAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateCupboardDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateCupboardDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={cupboardFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
