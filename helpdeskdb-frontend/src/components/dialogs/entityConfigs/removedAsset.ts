import {
	IAsset,
	IRemovedAsset,
	IRemovedAssetWithAssetName,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
	toOptions,
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
	toOptions(assets, (asset) => asset.assetName);

export const removedAssetToForm = (
	removedAsset: IRemovedAssetWithAssetName,
): RemovedAssetForm => ({
	assetId: removedAsset.assetId,
	comment: removedAsset.comment ?? "",
});

// Explicit field list — assetName is not part of the update body, and
// removedBy is carried over from the existing record.
export const removedAssetToUpdate = (
	form: RemovedAssetForm,
	entity: IRemovedAssetWithAssetName,
): IRemovedAsset => ({
	id: entity.id,
	assetId: form.assetId,
	comment: form.comment,
	removedBy: entity.removedBy,
});

export const removedAssetDeleteSummary: DeleteSummaryField<IRemovedAssetWithAssetName>[] =
	[
		{ labelKey: "Asset", render: (ra) => ra.assetName },
		{ labelKey: "common:Comment", render: (ra) => ra.comment || "-" },
		{ labelKey: "RemovedBy", render: (ra) => ra.removedBy },
	];
