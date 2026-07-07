import { useMemo } from "react";
import { IAsset, ILocation } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	locationAssetFormConfig,
	locationsToOptions,
} from "../entityConfigs/locationAsset";

interface CreateLocationAssetDialogProps {
	open: boolean;
	assets: IAsset[];
	locations: ILocation[];
	onClose: () => void;
	onConfirm: (data: {
		assetId: string;
		locationId: string;
	}) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateLocationAssetDialog = ({
	open,
	assets,
	locations,
	onClose,
	onConfirm,
	isLoading,
}: CreateLocationAssetDialogProps) => {
	const options = useMemo(
		() => ({
			assets: assetsToOptions(assets),
			locations: locationsToOptions(locations),
		}),
		[assets, locations],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={locationAssetFormConfig}
			options={options}
			onClose={onClose}
			onConfirm={onConfirm}
			isLoading={isLoading}
		/>
	);
};
