import { IRoleAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { roleFormConfig } from "../entityConfigs/role";

interface CreateRoleDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IRoleAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateRoleDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateRoleDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={roleFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
