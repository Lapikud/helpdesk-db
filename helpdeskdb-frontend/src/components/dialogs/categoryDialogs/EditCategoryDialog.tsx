import { useMemo } from "react";
import { ICategory } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { categoryFormConfig, categoryToForm } from "../entityConfigs/category";

interface EditCategoryDialogProps {
	open: boolean;
	category: ICategory | null;
	onClose: () => void;
	onConfirm: (data: ICategory) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditCategoryDialog = ({
	open,
	category,
	onClose,
	onConfirm,
	isLoading,
}: EditCategoryDialogProps) => {
	const initialValues = useMemo(
		() => (category ? categoryToForm(category) : null),
		[category],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={categoryFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) => onConfirm({ id: category!.id, ...data })}
			isLoading={isLoading}
		/>
	);
};
