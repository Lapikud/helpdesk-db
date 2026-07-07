import { useMemo } from "react";
import { IAsset, IOwner, IOwnerAssetAdd } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	ownerAssetCreateConfig,
	ownersToOptions,
} from "../entityConfigs/ownerAsset";

interface CreateOwnerAssetDialogProps {
	open: boolean;
	assets: IAsset[];
	owners: IOwner[];
	onClose: () => void;
	onConfirm: (data: IOwnerAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateOwnerAssetDialog = ({
	open,
	assets,
	owners,
	onClose,
	onConfirm,
	isLoading,
}: CreateOwnerAssetDialogProps) => {
	const options = useMemo(
		() => ({
			assets: assetsToOptions(assets),
			owners: ownersToOptions(owners),
		}),
		[assets, owners],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={ownerAssetCreateConfig}
			options={options}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					assetId: data.assetId,
					ownerId: data.ownerId,
					createdBy: "",
				})
			}
			isLoading={isLoading}
		/>
	);
};
