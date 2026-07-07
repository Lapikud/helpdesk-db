import { IOwnerAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { ownerFormConfig } from "../entityConfigs/owner";

interface CreateOwnerDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IOwnerAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateOwnerDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateOwnerDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={ownerFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
