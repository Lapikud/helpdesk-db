import { IRole } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { roleDeleteSummary } from "../entityConfigs/role";

interface DeleteRoleDialogProps {
	open: boolean;
	role: IRole | null;
	onClose: () => void;
	onConfirm: (roleId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteRoleDialog = ({
	open,
	role,
	onClose,
	onConfirm,
	isLoading,
}: DeleteRoleDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={role}
		namespace="approle"
		singularKey="AppRoleSingular"
		summaryFields={roleDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
