import {
	IAsset,
	ILocation,
	ILocationAssetWithNames,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
} from "../common/entityDialogTypes";

export type LocationAssetForm = {
	assetId: string;
	locationId: string;
};

export const locationAssetFormConfig: FormDialogConfig<LocationAssetForm> = {
	namespace: "locationassets",
	singularKey: "LocationAssetsSingular",
	defaultValues: { assetId: "", locationId: "" },
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
			name: "locationId",
			labelKey: "Location",
			optionsKey: "locations",
			required: true,
		},
	],
};

export const assetsToOptions = (assets: IAsset[]): SelectOption[] =>
	assets.map((asset) => ({ value: asset.id, label: asset.assetName }));

export const locationsToOptions = (locations: ILocation[]): SelectOption[] =>
	locations.map((location) => ({
		value: location.id,
		label: location.locationName,
	}));

export const locationAssetDeleteSummary: DeleteSummaryField<ILocationAssetWithNames>[] =
	[
		{ labelKey: "Asset", render: (la) => la.assetName },
		{ labelKey: "Location", render: (la) => la.locationName },
	];
