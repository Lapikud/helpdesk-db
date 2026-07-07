import { ICupboard, ICupboardAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const cupboardFormConfig: FormDialogConfig<ICupboardAdd> = {
	namespace: "cupboard",
	singularKey: "CupboardSingular",
	defaultValues: { codeName: "" },
	fields: [
		{
			kind: "text",
			name: "codeName",
			labelKey: "CodeName",
			placeholderKey: "CodeNamePrompt",
			validation: { required: true, minLength: 2, maxLength: 128 },
		},
	],
};

export const cupboardToForm = (cupboard: ICupboard): ICupboardAdd => ({
	codeName: cupboard.codeName,
});

export const cupboardDeleteSummary: DeleteSummaryField<ICupboard>[] = [
	{ labelKey: "CodeName", render: (c) => c.codeName },
];
