import {
	IAsset,
	IRemovedAssetWithAssetName,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
} from "../common/entityDialogTypes";

export type RemovedAssetForm = {
	assetId: string;
	comment: string;
};

export const removedAssetCreateConfig: FormDialogConfig<RemovedAssetForm> = {
	namespace: "removedassets",
	singularKey: "RemovedAssetsSingular",
	defaultValues: { assetId: "", comment: "" },
	fields: [
		{
			kind: "select",
			name: "assetId",
			labelKey: "Asset",
			optionsKey: "assets",
			required: true,
			selectArticleKey: "SelectAn",
		},
		{ kind: "readonly", labelKey: "User", valueKey: "userName" },
		{
			kind: "text",
			name: "comment",
			labelKey: "common:Comment",
			placeholderKey: "common:CommentPrompt",
			validation: { required: true, minLength: 2, maxLength: 255 },
		},
	],
};

export const removedAssetEditConfig: FormDialogConfig<RemovedAssetForm> = {
	namespace: "removedassets",
	singularKey: "RemovedAssetsSingular",
	defaultValues: { assetId: "", comment: "" },
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
			kind: "text",
			name: "comment",
			labelKey: "common:Comment",
			placeholderKey: "common:CommentPrompt",
			validation: { required: true, minLength: 2, maxLength: 255 },
		},
	],
};

export const assetsToOptions = (assets: IAsset[]): SelectOption[] =>
	assets.map((asset) => ({ value: asset.id, label: asset.assetName }));

export const removedAssetDeleteSummary: DeleteSummaryField<IRemovedAssetWithAssetName>[] =
	[
		{ labelKey: "Asset", render: (ra) => ra.assetName },
		{ labelKey: "common:Comment", render: (ra) => ra.comment || "-" },
		{ labelKey: "RemovedBy", render: (ra) => ra.removedBy },
	];
