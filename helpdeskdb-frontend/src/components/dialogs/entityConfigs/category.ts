import { ICategory, ICategoryAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const categoryFormConfig: FormDialogConfig<ICategoryAdd> = {
	namespace: "category",
	singularKey: "CategorySingular",
	defaultValues: { categoryName: "", comment: "" },
	fields: [
		{
			kind: "text",
			name: "categoryName",
			labelKey: "CategoryName",
			placeholderKey: "CategoryNamePrompt",
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

export const categoryToForm = (category: ICategory): ICategoryAdd => ({
	categoryName: category.categoryName,
	comment: category.comment,
});

export const categoryDeleteSummary: DeleteSummaryField<ICategory>[] = [
	{ labelKey: "CategoryName", render: (c) => c.categoryName },
	{ labelKey: "common:Comment", render: (c) => c.comment || "-" },
];
