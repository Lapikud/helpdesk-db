import { IAsset } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export type DbAssetForm = {
	assetName: string;
	comment: string;
	serialNumber: string;
	barcode: string;
};

export const dbAssetFormConfig: FormDialogConfig<DbAssetForm> = {
	namespace: "asset",
	singularKey: "AssetSingular",
	defaultValues: { assetName: "", comment: "", serialNumber: "", barcode: "" },
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
	],
};

export const dbAssetToForm = (asset: IAsset): DbAssetForm => ({
	assetName: asset.assetName,
	comment: asset.comment,
	serialNumber: asset.serialNumber ?? "",
	barcode: asset.barcode ?? "",
});

export const dbAssetDeleteSummary: DeleteSummaryField<IAsset>[] = [
	{ labelKey: "AssetName", render: (a) => a.assetName },
	{ labelKey: "SerialNumber", render: (a) => a.serialNumber || "-" },
	{ labelKey: "Barcode", render: (a) => a.barcode || "-" },
	{ labelKey: "common:Comment", render: (a) => a.comment || "-" },
];
