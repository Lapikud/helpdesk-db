import { useMemo } from "react";
import { IRole } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { roleFormConfig, roleToForm } from "../entityConfigs/role";

interface EditRoleDialogProps {
	open: boolean;
	role: IRole | null;
	onClose: () => void;
	onConfirm: (data: IRole) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditRoleDialog = ({
	open,
	role,
	onClose,
	onConfirm,
	isLoading,
}: EditRoleDialogProps) => {
	const initialValues = useMemo(
		() => (role ? roleToForm(role) : null),
		[role],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={roleFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: role!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
