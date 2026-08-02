import {
	ICategory,
	ICategoryAsset,
	ILocation,
	ILocationAsset,
	IOwner,
	IOwnerAsset,
} from "@/types/domain/DomainTypes";
import {
	IAssetViewModel,
	IAssetViewModelUpdate,
} from "@/types/domain/IAssetViewModels";
import {
	FormDialogConfig,
	SelectOption,
	toOptions,
} from "../common/entityDialogTypes";

export type EditAssetForm = {
	assetName: string;
	comment: string;
	serialNumber: string;
	barcode: string;
	categoryId: string;
	ownerId: string;
	locationId: string;
};

export const editAssetFormConfig: FormDialogConfig<EditAssetForm> = {
	namespace: "asset",
	singularKey: "AssetSingular",
	defaultValues: {
		assetName: "",
		comment: "",
		serialNumber: "",
		barcode: "",
		categoryId: "",
		ownerId: "",
		locationId: "",
	},
	fields: [
		{
			kind: "text",
			name: "assetName",
			labelKey: "AssetName",
			placeholderKey: "AssetNamePrompt",
			validation: { required: true, minLength: 2, maxLength: 128 },
		},
		{
			kind: "text",
			name: "comment",
			labelKey: "common:Comment",
			placeholderKey: "common:CommentPrompt",
			validation: { required: true, minLength: 2, maxLength: 255 },
		},
		{
			kind: "text",
			name: "serialNumber",
			labelKey: "SerialNumber",
			placeholderKey: "SerialNumberPrompt",
			validation: { maxLength: 255 },
		},
		{
			kind: "text",
			name: "barcode",
			labelKey: "Barcode",
			placeholderKey: "BarcodePrompt",
			validation: { maxLength: 255 },
		},
		{
			kind: "select",
			name: "categoryId",
			labelKey: "assetviewmodel:Category",
			optionsKey: "categories",
			required: true,
		},
		{
			kind: "select",
			name: "ownerId",
			labelKey: "assetviewmodel:Owner",
			optionsKey: "owners",
			required: true,
			selectArticleKey: "SelectAn",
		},
		{
			kind: "select",
			name: "locationId",
			labelKey: "assetviewmodel:Location",
			optionsKey: "locations",
			required: true,
		},
	],
};

export const editAssetToForm = (
	asset: IAssetViewModel,
	comment: string,
	categoryAssets: ICategoryAsset | null,
	ownerAssets: IOwnerAsset | null,
	locationAssets: ILocationAsset | null,
): EditAssetForm => ({
	assetName: asset.assetName,
	comment,
	serialNumber: asset.serialNumber ?? "",
	barcode: asset.barcode ?? "",
	categoryId: categoryAssets?.categoryId ?? "",
	ownerId: ownerAssets?.ownerId ?? "",
	locationId: locationAssets?.locationId ?? "",
});

export const editAssetToUpdate = (
	form: EditAssetForm,
	asset: IAssetViewModel,
	categoryAssets: ICategoryAsset | null,
	ownerAssets: IOwnerAsset | null,
	locationAssets: ILocationAsset | null,
): IAssetViewModelUpdate => ({
	assetId: asset.id,
	assetName: form.assetName,
	comment: form.comment,
	serialNumber: form.serialNumber || null,
	barcode: form.barcode || null,
	selectedCategoryId: form.categoryId,
	selectedOwnerId: form.ownerId,
	selectedLocationId: form.locationId,
	categoryAssetsId: categoryAssets?.id ?? null,
	ownerAssetsId: ownerAssets?.id ?? null,
	locationAssetsId: locationAssets?.id ?? null,
});

export const categoriesToOptions = (categories: ICategory[]): SelectOption[] =>
	toOptions(categories, (c) => c.categoryName);

export const ownersToOptions = (owners: IOwner[]): SelectOption[] =>
	toOptions(owners, (o) => o.ownerName);

export const locationsToOptions = (locations: ILocation[]): SelectOption[] =>
	toOptions(locations, (l) => l.locationName);
