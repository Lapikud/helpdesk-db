import {
	ILocation,
	ILocationInCupboardWithNames,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
} from "../common/entityDialogTypes";

export type LocationInCupboardForm = {
	locationId: string;
};

export const locationInCupboardFormConfig: FormDialogConfig<LocationInCupboardForm> =
	{
		namespace: "locationincupboard",
		singularKey: "LocationInCupboardSingular",
		defaultValues: { locationId: "" },
		fields: [
			{ kind: "readonly", labelKey: "Cupboard", valueKey: "cupboardCodeName" },
			{
				kind: "select",
				name: "locationId",
				labelKey: "Location",
				optionsKey: "locations",
				required: true,
			},
		],
	};

export const locationsToOptions = (locations: ILocation[]): SelectOption[] =>
	locations.map((location) => ({
		value: location.id,
		label: location.locationName,
	}));

export const locationInCupboardDeleteSummary: DeleteSummaryField<ILocationInCupboardWithNames>[] =
	[
		{ labelKey: "Cupboard", render: (entry) => entry.codeName },
		{ labelKey: "Location", render: (entry) => entry.locationName },
	];
