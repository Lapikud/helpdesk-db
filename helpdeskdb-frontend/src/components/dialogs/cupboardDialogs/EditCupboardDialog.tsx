import { useMemo } from "react";
import { ICupboard } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { cupboardFormConfig, cupboardToForm } from "../entityConfigs/cupboard";

interface EditCupboardDialogProps {
	open: boolean;
	cupboard: ICupboard | null;
	onClose: () => void;
	onConfirm: (data: ICupboard) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditCupboardDialog = ({
	open,
	cupboard,
	onClose,
	onConfirm,
	isLoading,
}: EditCupboardDialogProps) => {
	const initialValues = useMemo(
		() => (cupboard ? cupboardToForm(cupboard) : null),
		[cupboard],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={cupboardFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: cupboard!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
