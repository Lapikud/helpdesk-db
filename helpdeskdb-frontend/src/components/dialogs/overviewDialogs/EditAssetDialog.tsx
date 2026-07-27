import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import {
	IAssetViewModel,
	IAssetViewModelUpdate,
} from "@/types/domain/IAssetViewModels";
import {
	ICategory,
	ICategoryAsset,
	ILocation,
	ILocationAsset,
	IOwner,
	IOwnerAsset,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	EditAssetForm,
	categoriesToOptions,
	editAssetFormConfig,
	editAssetToForm,
	editAssetToUpdate,
	locationsToOptions,
	ownersToOptions,
} from "../entityConfigs/editAssetViewModel";

interface IEditAssetDialogProps {
	open: boolean;
	asset: IAssetViewModel | null;
	comment: string;
	categoryAssets: ICategoryAsset | null;
	ownerAssets: IOwnerAsset | null;
	locationAssets: ILocationAsset | null;
	categories: ICategory[];
	locations: ILocation[];
	owners: IOwner[];
	onClose: () => void;
	onConfirm: (
		assetId: string,
		updateAsset: IAssetViewModelUpdate
	) => Promise<void>;
	isLoading: boolean;
}

export const EditAssetDialog = ({
	open,
	asset,
	comment,
	categoryAssets,
	ownerAssets,
	locationAssets,
	categories,
	locations,
	owners,
	onClose,
	onConfirm,
	isLoading,
}: IEditAssetDialogProps) => {
	// The config's category/owner/location labels live in the
	// "assetviewmodel" namespace; load it here so the generic dialog's
	// cross-namespace lookups resolve without relying on the parent page.
	useTranslation("assetviewmodel");

	const options = useMemo(
		() => ({
			categories: categoriesToOptions(categories),
			owners: ownersToOptions(owners),
			locations: locationsToOptions(locations),
		}),
		[categories, owners, locations],
	);

	const initialValues = useMemo(
		() =>
			asset
				? editAssetToForm(
						asset,
						comment,
						categoryAssets,
						ownerAssets,
						locationAssets,
					)
				: null,
		[asset, comment, categoryAssets, ownerAssets, locationAssets],
	);

	const handleConfirm = async (form: EditAssetForm) => {
		if (!asset) return;
		try {
			await onConfirm(
				asset.id,
				editAssetToUpdate(
					form,
					asset,
					categoryAssets,
					ownerAssets,
					locationAssets,
				),
			);
		} catch (error) {
			return { error: (error as Error).message };
		}
	};

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={editAssetFormConfig}
			initialValues={initialValues}
			options={options}
			onClose={onClose}
			onConfirm={handleConfirm}
			isLoading={isLoading}
		/>
	);
};
