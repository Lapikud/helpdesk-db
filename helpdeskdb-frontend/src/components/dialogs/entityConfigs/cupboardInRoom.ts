import {
	ICupboard,
	ICupboardInRoomAdd,
	ICupboardInRoomWithNames,
	IRoom,
} from "@/types/domain/DomainTypes";
import {
	DeleteSummaryField,
	FormDialogConfig,
	SelectOption,
} from "../common/entityDialogTypes";

export type CupboardInRoomEditForm = {
	roomId: string;
	comment: string;
};

export const cupboardInRoomCreateConfig: FormDialogConfig<ICupboardInRoomAdd> =
	{
		namespace: "cupboardinroom",
		singularKey: "CupboardInRoomSingular",
		defaultValues: { cupboardId: "", roomId: "", comment: "" },
		fields: [
			{
				kind: "select",
				name: "cupboardId",
				labelKey: "Cupboard",
				optionsKey: "cupboards",
				required: true,
			},
			{
				kind: "select",
				name: "roomId",
				labelKey: "Room",
				optionsKey: "rooms",
				required: true,
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

export const cupboardInRoomEditConfig: FormDialogConfig<CupboardInRoomEditForm> =
	{
		namespace: "cupboardinroom",
		singularKey: "CupboardInRoomSingular",
		defaultValues: { roomId: "", comment: "" },
		fields: [
			{ kind: "readonly", labelKey: "Cupboard", valueKey: "codeName" },
			{
				kind: "select",
				name: "roomId",
				labelKey: "Room",
				optionsKey: "rooms",
				required: true,
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

export const cupboardsToOptions = (cupboards: ICupboard[]): SelectOption[] =>
	cupboards.map((cupboard) => ({
		value: cupboard.id,
		label: cupboard.codeName,
	}));

export const roomsToOptions = (rooms: IRoom[]): SelectOption[] =>
	rooms.map((room) => ({ value: room.id, label: room.roomName }));

export const cupboardInRoomDeleteSummary: DeleteSummaryField<ICupboardInRoomWithNames>[] =
	[
		{ labelKey: "Room", render: (entry) => entry.roomName },
		{ labelKey: "Cupboard", render: (entry) => entry.codeName },
		{ labelKey: "common:Comment", render: (entry) => entry.comment },
	];
