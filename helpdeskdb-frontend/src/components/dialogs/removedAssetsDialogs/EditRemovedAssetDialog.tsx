import { useMemo } from "react";
import {
	IAsset,
	IRemovedAsset,
	IRemovedAssetWithAssetName,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	removedAssetEditConfig,
} from "../entityConfigs/removedAsset";

interface EditRemovedAssetDialogProps {
	open: boolean;
	removedAsset: IRemovedAssetWithAssetName | null;
	assets: IAsset[];
	onClose: () => void;
	onConfirm: (data: IRemovedAsset) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditRemovedAssetDialog = ({
	open,
	removedAsset,
	assets,
	onClose,
	onConfirm,
	isLoading,
}: EditRemovedAssetDialogProps) => {
	// The removed asset's own asset is excluded from the "available assets"
	// list the page passes in, so add it back or the select can't show the
	// current value.
	const options = useMemo(() => {
		const withCurrent =
			!removedAsset || assets.some((a) => a.id === removedAsset.assetId)
				? assets
				: [
						...assets,
						{
							id: removedAsset.assetId,
							assetName: removedAsset.assetName,
						} as IAsset,
				  ];
		return { assets: assetsToOptions(withCurrent) };
	}, [assets, removedAsset]);

	const initialValues = useMemo(
		() =>
			removedAsset
				? {
						assetId: removedAsset.assetId,
						comment: removedAsset.comment ?? "",
				  }
				: null,
		[removedAsset],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={removedAssetEditConfig}
			initialValues={initialValues}
			options={options}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					id: removedAsset!.id,
					assetId: data.assetId,
					comment: data.comment,
					removedBy: removedAsset!.removedBy,
				})
			}
			isLoading={isLoading}
		/>
	);
};
