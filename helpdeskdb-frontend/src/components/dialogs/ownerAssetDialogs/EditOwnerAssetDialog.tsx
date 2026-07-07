import { useMemo } from "react";
import { IOwner, IOwnerAssetWithNames } from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	ownerAssetEditConfig,
	ownersToOptions,
} from "../entityConfigs/ownerAsset";

interface EditOwnerAssetDialogProps {
	open: boolean;
	ownerAsset: IOwnerAssetWithNames | null;
	owners: IOwner[];
	onClose: () => void;
	onConfirm: (
		data: IOwnerAssetWithNames,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditOwnerAssetDialog = ({
	open,
	ownerAsset,
	owners,
	onClose,
	onConfirm,
	isLoading,
}: EditOwnerAssetDialogProps) => {
	const options = useMemo(() => ({ owners: ownersToOptions(owners) }), [
		owners,
	]);
	const initialValues = useMemo(
		() => (ownerAsset ? { ownerId: ownerAsset.ownerId } : null),
		[ownerAsset],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={ownerAssetEditConfig}
			initialValues={initialValues}
			options={options}
			staticValues={{ assetName: ownerAsset?.assetName ?? "" }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					...ownerAsset!,
					ownerId: data.ownerId,
				})
			}
			isLoading={isLoading}
		/>
	);
};
