import { ILocationAssetWithNames } from "@/types/domain/DomainTypes";
import { EntityDeleteDialog } from "../common/EntityDeleteDialog";
import { locationAssetDeleteSummary } from "../entityConfigs/locationAsset";

interface DeleteLocationAssetDialogProps {
	open: boolean;
	locationAsset: ILocationAssetWithNames | null;
	onClose: () => void;
	onConfirm: (id: string) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const DeleteLocationAssetDialog = ({
	open,
	locationAsset,
	onClose,
	onConfirm,
	isLoading,
}: DeleteLocationAssetDialogProps) => (
	<EntityDeleteDialog
		open={open}
		entity={locationAsset}
		namespace="locationassets"
		singularKey="LocationAssetsSingular"
		summaryFields={locationAssetDeleteSummary}
		onClose={onClose}
		onConfirm={onConfirm}
		isLoading={isLoading}
	/>
);
