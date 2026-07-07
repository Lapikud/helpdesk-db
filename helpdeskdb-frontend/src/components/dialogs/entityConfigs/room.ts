import { IRoom, IRoomAdd } from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
} from "../common/entityDialogTypes";

export const roomFormConfig: FormDialogConfig<IRoomAdd> = {
	namespace: "room",
	singularKey: "RoomSingular",
	defaultValues: { roomName: "", comment: "" },
	fields: [
		{
			kind: "text",
			name: "roomName",
			labelKey: "RoomName",
			placeholderKey: "RoomNamePrompt",
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

export const roomToForm = (room: IRoom): IRoomAdd => ({
	roomName: room.roomName,
	comment: room.comment,
});

export const roomDeleteSummary: DeleteSummaryField<IRoom>[] = [
	{ labelKey: "RoomName", render: (r) => r.roomName },
	{ labelKey: "common:Comment", render: (r) => r.comment || "-" },
];
