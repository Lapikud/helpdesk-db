import { ICategoryAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { categoryFormConfig } from "../entityConfigs/category";

interface CreateCategoryDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: ICategoryAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateCategoryDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateCategoryDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={categoryFormConfig}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
