import { useMemo } from "react";
import {
	IAsset,
	ICategory,
	ICategoryAssetAdd,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	assetsToOptions,
	categoriesToOptions,
	categoryAssetCreateConfig,
	categoryAssetToAdd,
} from "../entityConfigs/categoryAsset";

interface CreateCategoryAssetDialogProps {
	open: boolean;
	assets: IAsset[];
	categories: ICategory[];
	onClose: () => void;
	onConfirm: (data: ICategoryAssetAdd) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const CreateCategoryAssetDialog = ({
	open,
	assets,
	categories,
	onClose,
	onConfirm,
	isLoading,
}: CreateCategoryAssetDialogProps) => {
	const options = useMemo(
		() => ({
			assets: assetsToOptions(assets),
			categories: categoriesToOptions(categories),
		}),
		[assets, categories],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="create"
			config={categoryAssetCreateConfig}
			options={options}
			onClose={onClose}
			onConfirm={(data) => onConfirm(categoryAssetToAdd(data))}
			isLoading={isLoading}
		/>
	);
};
