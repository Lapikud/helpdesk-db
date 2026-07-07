import { IRole, IRoleAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const roleFormConfig: FormDialogConfig<IRoleAdd> = {
	namespace: "approle",
	singularKey: "AppRoleSingular",
	defaultValues: { name: "" },
	fields: [
		{
			kind: "text",
			name: "name",
			labelKey: "AppRoleName",
			placeholderKey: "AppRoleNamePrompt",
			validation: { required: true, minLength: 2, maxLength: 128 },
		},
	],
};

export const roleToForm = (role: IRole): IRoleAdd => ({
	name: role.name,
});

export const roleDeleteSummary: DeleteSummaryField<IRole>[] = [
	{ labelKey: "AppRoleName", render: (r) => r.name },
];
