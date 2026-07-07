import {
	IAsset,
	IOwner,
	IOwnerAssetWithNames,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
} from "../common/entityDialogTypes";

export type OwnerAssetCreateForm = {
	assetId: string;
	ownerId: string;
};

export type OwnerAssetEditForm = {
	ownerId: string;
};

export const ownerAssetCreateConfig: FormDialogConfig<OwnerAssetCreateForm> = {
	namespace: "ownerassets",
	singularKey: "OwnerAssetsSingular",
	defaultValues: { assetId: "", ownerId: "" },
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
			name: "ownerId",
			labelKey: "Owner",
			optionsKey: "owners",
			required: true,
			selectArticleKey: "SelectAn",
		},
	],
};

export const ownerAssetEditConfig: FormDialogConfig<OwnerAssetEditForm> = {
	namespace: "ownerassets",
	singularKey: "OwnerAssetsSingular",
	defaultValues: { ownerId: "" },
	fields: [
		{ kind: "display", labelKey: "Asset", valueKey: "assetName" },
		{
			kind: "select",
			name: "ownerId",
			labelKey: "Owner",
			optionsKey: "owners",
			required: true,
			selectArticleKey: "SelectAn",
		},
	],
};

export const assetsToOptions = (assets: IAsset[]): SelectOption[] =>
	assets.map((asset) => ({ value: asset.id, label: asset.assetName }));

export const ownersToOptions = (owners: IOwner[]): SelectOption[] =>
	owners.map((owner) => ({ value: owner.id, label: owner.ownerName }));

export const ownerAssetDeleteSummary: DeleteSummaryField<IOwnerAssetWithNames>[] =
	[
		{ labelKey: "Asset", render: (oa) => oa.assetName },
		{ labelKey: "Owner", render: (oa) => oa.ownerName },
		{ labelKey: "common:CreatedBy", render: (oa) => oa.createdBy || "-" },
	];
