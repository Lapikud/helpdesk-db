import { useMemo } from "react";
import {
	IAsset,
	ILocation,
	ILocationAsset,
	ILocationAssetWithNames,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	locationAssetFormConfig,
	locationsToOptions,
} from "../entityConfigs/locationAsset";

interface EditLocationAssetDialogProps {
	open: boolean;
	locationAsset: ILocationAssetWithNames | null;
	assets: IAsset[];
	locations: ILocation[];
	onClose: () => void;
	onConfirm: (data: ILocationAsset) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditLocationAssetDialog = ({
	open,
	locationAsset,
	assets,
	locations,
	onClose,
	onConfirm,
	isLoading,
}: EditLocationAssetDialogProps) => {
	const options = useMemo(
		() => ({
			assets: assetsToOptions(assets),
			locations: locationsToOptions(locations),
		}),
		[assets, locations],
	);
	const initialValues = useMemo(
		() =>
			locationAsset
				? {
						assetId: locationAsset.assetId,
						locationId: locationAsset.locationId,
				  }
				: null,
		[locationAsset],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={locationAssetFormConfig}
			initialValues={initialValues}
			options={options}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					id: locationAsset!.id,
					assetId: data.assetId,
					locationId: data.locationId,
					createdBy: locationAsset!.createdBy,
				})
			}
			isLoading={isLoading}
		/>
	);
};
