import {
	IAsset,
	ICategory,
	ICategoryAssetAdd,
	ICategoryAssetWithNames,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
	toOptions,
} from "../common/entityDialogTypes";

export type CategoryAssetCreateForm = {
	assetId: string;
	categoryId: string;
	comment: string;
};

export type CategoryAssetEditForm = {
	categoryId: string;
	comment: string;
};

export const categoryAssetCreateConfig: FormDialogConfig<CategoryAssetCreateForm> =
	{
		namespace: "categoryassets",
		singularKey: "CategoryAssetsSingular",
		defaultValues: { assetId: "", categoryId: "", comment: "" },
		fields: [
			{
				kind: "select",
				name: "assetId",
				labelKey: "Asset",
				optionsKey: "assets",
				required: true,
				selectArticleKey: "SelectAn",
			},
			{
				kind: "select",
				name: "categoryId",
				labelKey: "Category",
				optionsKey: "categories",
				required: true,
			},
			{
				kind: "text",
				name: "comment",
				labelKey: "common:Comment",
				placeholderKey: "common:CommentPrompt",
				validation: { required: true, minLength: 2, maxLength: 255 },
			},
		],
	};

export const categoryAssetEditConfig: FormDialogConfig<CategoryAssetEditForm> =
	{
		namespace: "categoryassets",
		singularKey: "CategoryAssetsSingular",
		defaultValues: { categoryId: "", comment: "" },
		fields: [
			{ kind: "display", labelKey: "Asset", valueKey: "assetName" },
			{
				kind: "select",
				name: "categoryId",
				labelKey: "Category",
				optionsKey: "categories",
				required: true,
			},
			{
				kind: "text",
				name: "comment",
				labelKey: "common:Comment",
				placeholderKey: "common:CommentPrompt",
				validation: { required: true, minLength: 2, maxLength: 255 },
			},
		],
	};

export const assetsToOptions = (assets: IAsset[]): SelectOption[] =>
	toOptions(assets, (asset) => asset.assetName);

export const categoriesToOptions = (categories: ICategory[]): SelectOption[] =>
	toOptions(categories, (category) => category.categoryName);

export const categoryAssetDeleteSummary: DeleteSummaryField<ICategoryAssetWithNames>[] =
	[
		{ labelKey: "Asset", render: (ca) => ca.assetName },
		{ labelKey: "Category", render: (ca) => ca.categoryName },
		{ labelKey: "common:Comment", render: (ca) => ca.comment || "-" },
		{ labelKey: "common:CreatedBy", render: (ca) => ca.createdBy || "-" },
	];

export const categoryAssetToAdd = (
	data: CategoryAssetCreateForm,
): ICategoryAssetAdd => ({
	assetId: data.assetId,
	categoryId: data.categoryId,
	comment: data.comment,
	createdBy: "",
});
