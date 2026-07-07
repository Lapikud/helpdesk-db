import { useMemo } from "react";
import { IOwner } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { ownerFormConfig, ownerToForm } from "../entityConfigs/owner";

interface EditOwnerDialogProps {
	open: boolean;
	owner: IOwner | null;
	onClose: () => void;
	onConfirm: (data: IOwner) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditOwnerDialog = ({
	open,
	owner,
	onClose,
	onConfirm,
	isLoading,
}: EditOwnerDialogProps) => {
	const initialValues = useMemo(
		() => (owner ? ownerToForm(owner) : null),
		[owner],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={ownerFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: owner!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
