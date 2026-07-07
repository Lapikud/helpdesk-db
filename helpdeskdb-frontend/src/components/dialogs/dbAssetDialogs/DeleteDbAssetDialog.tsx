import { IAsset } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { dbAssetDeleteSummary } from "../entityConfigs/dbAsset";

interface DeleteDbAssetDialogProps {
	open: boolean;
	asset: IAsset | null;
	onClose: () => void;
	onConfirm: (assetId: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteDbAssetDialog = ({
	open,
	asset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteDbAssetDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={asset}
		namespace="asset"
		singularKey="AssetSingular"
		summaryFields={dbAssetDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
