import { ICategoryAssetWithNames } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { categoryAssetDeleteSummary } from "../entityConfigs/categoryAsset";

interface DeleteCategoryAssetDialogProps {
	open: boolean;
	categoryAsset: ICategoryAssetWithNames | null;
	onClose: () => void;
	onConfirm: (id: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteCategoryAssetDialog = ({
	open,
	categoryAsset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteCategoryAssetDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={categoryAsset}
		namespace="categoryassets"
		singularKey="CategoryAssetsSingular"
		summaryFields={categoryAssetDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
