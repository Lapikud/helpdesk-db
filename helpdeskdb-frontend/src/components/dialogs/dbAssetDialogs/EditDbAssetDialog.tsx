import { useMemo } from "react";
import { IAsset } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import { dbAssetFormConfig, dbAssetToForm } from "../entityConfigs/dbAsset";

interface EditDbAssetDialogProps {
	open: boolean;
	asset: IAsset | null;
	onClose: () => void;
	onConfirm: (data: IAsset) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditDbAssetDialog = ({
	open,
	asset,
	onClose,
	onConfirm,
	isLoading,
}: EditDbAssetDialogProps) => {
	const initialValues = useMemo(
		() => (asset ? dbAssetToForm(asset) : null),
		[asset],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={dbAssetFormConfig}
			initialValues={initialValues}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					id: asset!.id,
					assetName: data.assetName,
					comment: data.comment,
					serialNumber: data.serialNumber || null,
					barcode: data.barcode || null,
				})
			}
			isLoading={isLoading}
		/>
	);
};
