import { IRemovedAssetWithAssetName } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { removedAssetDeleteSummary } from "../entityConfigs/removedAsset";

interface DeleteRemovedAssetDialogProps {
	open: boolean;
	removedAsset: IRemovedAssetWithAssetName | null;
	onClose: () => void;
	onConfirm: (removedAssetId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteRemovedAssetDialog = ({
	open,
	removedAsset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteRemovedAssetDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={removedAsset}
		namespace="removedassets"
		singularKey="RemovedAssetsSingular"
		summaryFields={removedAssetDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
