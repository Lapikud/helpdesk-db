import { ILocation, ILocationAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const locationFormConfig: FormDialogConfig<ILocationAdd> = {
	namespace: "location",
	singularKey: "LocationSingular",
	defaultValues: { locationName: "", shelfNum: 1, column: 1 },
	fields: [
		{
			kind: "text",
			name: "locationName",
			labelKey: "LocationName",
			placeholderKey: "LocationNamePrompt",
			validation: { required: true, minLength: 2, maxLength: 128 },
		},
		{
			kind: "number",
			name: "shelfNum",
			labelKey: "ShelfNum",
			placeholder: "1",
			validation: { required: true, min: 1, max: 5 },
		},
		{
			kind: "number",
			name: "column",
			labelKey: "Column",
			placeholder: "1",
			validation: { required: true, min: 1, max: 5 },
		},
	],
};

export const locationToForm = (location: ILocation): ILocationAdd => ({
	locationName: location.locationName,
	shelfNum: location.shelfNum,
	column: location.column,
});

export const locationDeleteSummary: DeleteSummaryField<ILocation>[] = [
	{ labelKey: "LocationName", render: (l) => l.locationName },
	{ labelKey: "ShelfNum", render: (l) => l.shelfNum },
	{ labelKey: "Column", render: (l) => l.column },
];
