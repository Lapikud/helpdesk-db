import { ICategory } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { categoryDeleteSummary } from "../entityConfigs/category";

interface DeleteCategoryDialogProps {
	open: boolean;
	category: ICategory | null;
	onClose: () => void;
	onConfirm: (categoryId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteCategoryDialog = ({
	open,
	category,
	onClose,
	onConfirm,
	isLoading,
}: DeleteCategoryDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={category}
		namespace="category"
		singularKey="CategorySingular"
		summaryFields={categoryDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
