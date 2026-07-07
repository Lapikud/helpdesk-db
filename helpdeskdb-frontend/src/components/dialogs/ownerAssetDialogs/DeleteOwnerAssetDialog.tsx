import { IOwnerAssetWithNames } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { ownerAssetDeleteSummary } from "../entityConfigs/ownerAsset";

interface DeleteOwnerAssetDialogProps {
	open: boolean;
	ownerAsset: IOwnerAssetWithNames | null;
	onClose: () => void;
	onConfirm: (id: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteOwnerAssetDialog = ({
	open,
	ownerAsset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteOwnerAssetDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={ownerAsset}
		namespace="ownerassets"
		singularKey="OwnerAssetsSingular"
		summaryFields={ownerAssetDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
