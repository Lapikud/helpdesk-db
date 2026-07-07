import { IAssetAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { dbAssetFormConfig } from "../entityConfigs/dbAsset";

interface CreateDbAssetDialogProps {
	open: boolean;
	onClose: () => void;
	onConfirm: (data: IAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateDbAssetDialog = ({
	open,
	onClose,
	onConfirm,
	isLoading,
}: CreateDbAssetDialogProps) => (
	<EntityFormDialog
		open={open}
		mode="create"
		config={dbAssetFormConfig}
		onClose={onClose}
		onConfirm={(data) =>
			onConfirm({
				assetName: data.assetName,
				comment: data.comment,
				serialNumber: data.serialNumber || null,
				barcode: data.barcode || null,
			})
		}
		isLoading={isLoading}
	/>
);
