import {
	IAsset,
	ILocation,
	ILocationAssetAdd,
	ILocationAssetWithNames,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
	toOptions,
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
	toOptions(assets, (asset) => asset.assetName);

export const locationsToOptions = (locations: ILocation[]): SelectOption[] =>
	toOptions(locations, (location) => location.locationName);

// createdBy is stamped by the page from AccountContext — the placeholder just
// satisfies the DTO shape.
export const locationAssetToAdd = (
	data: LocationAssetForm,
): ILocationAssetAdd => ({
	assetId: data.assetId,
	locationId: data.locationId,
	createdBy: "",
});

export const locationAssetDeleteSummary: DeleteSummaryField<ILocationAssetWithNames>[] =
	[
		{ labelKey: "Asset", render: (la) => la.assetName },
		{ labelKey: "Location", render: (la) => la.locationName },
	];
