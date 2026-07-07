import { useMemo } from "react";
import {
	ICategory,
	ICategoryAssetWithNames,
} from "@/types/domain/DomainTypes";
import { EntityFormDialog } from "../common/EntityFormDialog";
import {
	categoriesToOptions,
	categoryAssetEditConfig,
} from "../entityConfigs/categoryAsset";

interface EditCategoryAssetDialogProps {
	open: boolean;
	categoryAsset: ICategoryAssetWithNames | null;
	categories: ICategory[];
	onClose: () => void;
	onConfirm: (
		data: ICategoryAssetWithNames,
	) => Promise<{ error?: string } | void>;
	isLoading: boolean;
}

export const EditCategoryAssetDialog = ({
	open,
	categoryAsset,
	categories,
	onClose,
	onConfirm,
	isLoading,
}: EditCategoryAssetDialogProps) => {
	const options = useMemo(
		() => ({ categories: categoriesToOptions(categories) }),
		[categories],
	);
	const initialValues = useMemo(
		() =>
			categoryAsset
				? {
						categoryId: categoryAsset.categoryId,
						comment: categoryAsset.comment ?? "",
				  }
				: null,
		[categoryAsset],
	);

	return (
		<EntityFormDialog
			open={open}
			mode="edit"
			config={categoryAssetEditConfig}
			initialValues={initialValues}
			options={options}
			staticValues={{ assetName: categoryAsset?.assetName ?? "" }}
			onClose={onClose}
			onConfirm={(data) =>
				onConfirm({
					...categoryAsset!,
					categoryId: data.categoryId,
					comment: data.comment,
				})
			}
			isLoading={isLoading}
		/>
	);
};
