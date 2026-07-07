import { IOwner, IOwnerAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const ownerFormConfig: FormDialogConfig<IOwnerAdd> = {
	namespace: "owner",
	singularKey: "OwnerSingular",
	defaultValues: { ownerName: "", comment: "" },
	fields: [
		{
			kind: "text",
			name: "ownerName",
			labelKey: "OwnerName",
			placeholderKey: "OwnerNamePrompt",
			validation: { required: true, minLength: 2, maxLength: 128 },
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

export const ownerToForm = (owner: IOwner): IOwnerAdd => ({
	ownerName: owner.ownerName,
	comment: owner.comment,
});

export const ownerDeleteSummary: DeleteSummaryField<IOwner>[] = [
	{ labelKey: "OwnerName", render: (o) => o.ownerName },
	{ labelKey: "common:Comment", render: (o) => o.comment || "-" },
];
